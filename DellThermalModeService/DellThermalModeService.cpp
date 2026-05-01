// DellThermalModeService.cpp
// Command-line tool to set Dell thermal mode via WMI (BFn.DoBFn)
// Run at boot via Task Scheduler or registry Run key

#define WIN32_LEAN_AND_MEAN
#define _WIN32_WINNT 0x0A00

#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <wbemidl.h>
#include <comdef.h>
#include <time.h>
#include <string>

#pragma comment(lib, "wbemuuid.lib")

// ============================================================================
// Dell SMI Object Structure (must match C# SmiObject, Pack=1)
// ============================================================================
#pragma pack(push, 1)
struct DellSmiObject {
    USHORT  Class;      // 0=TokenRead, 1=TokenWrite, 17=Info
    USHORT  Selector;   // 0=Standard, 19=ThermalMode
    ULONG   Input1;
    ULONG   Input2;
    ULONG   Input3;
    ULONG   Input4;
    ULONG   Output1;
    ULONG   Output2;
    ULONG   Output3;
    ULONG   Output4;
};
#pragma pack(pop)

static_assert(sizeof(DellSmiObject) == 36, "DellSmiObject must be 36 bytes");

// Thermal mode values (match ThermalSetting.cs)
enum class ThermalMode : ULONG {
    Error       = 0,
    Optimized   = 1,
    Cool        = 2,
    Quiet       = 4,
    Performance = 8
};

// ============================================================================
// Logging (to file only, no console output for silent boot execution)
// ============================================================================
static HANDLE g_hLogFile = INVALID_HANDLE_VALUE;
static CRITICAL_SECTION g_logCs;

void EnsureLogDirectory() {
    wchar_t path[MAX_PATH];
    wcscpy_s(path, L"C:\\ProgramData\\DellThermalMode");
    CreateDirectoryW(path, NULL);
}

void LogInit() {
    InitializeCriticalSection(&g_logCs);
    EnsureLogDirectory();
    g_hLogFile = CreateFileW(L"C:\\ProgramData\\DellThermalMode\\set_thermal_mode.log",
                              GENERIC_WRITE, FILE_SHARE_READ, NULL,
                              OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (g_hLogFile != INVALID_HANDLE_VALUE) {
        SetFilePointer(g_hLogFile, 0, NULL, FILE_END);
    }
}

void LogClose() {
    if (g_hLogFile != INVALID_HANDLE_VALUE) {
        CloseHandle(g_hLogFile);
        g_hLogFile = INVALID_HANDLE_VALUE;
    }
    DeleteCriticalSection(&g_logCs);
}

void LogMessage(const wchar_t* level, const wchar_t* fmt, ...) {
    if (g_hLogFile == INVALID_HANDLE_VALUE) return;

    EnterCriticalSection(&g_logCs);

    SYSTEMTIME st;
    GetLocalTime(&st);
    wchar_t timestamp[64];
    swprintf_s(timestamp, L"[%04d-%02d-%02d %02d:%02d:%02d] [%s] ",
               st.wYear, st.wMonth, st.wDay,
               st.wHour, st.wMinute, st.wSecond, level);

    wchar_t msg[2048];
    va_list args;
    va_start(args, fmt);
    vswprintf_s(msg, fmt, args);
    va_end(args);

    DWORD written;
    WriteFile(g_hLogFile, timestamp, (DWORD)(wcslen(timestamp) * sizeof(wchar_t)), &written, NULL);
    WriteFile(g_hLogFile, msg, (DWORD)(wcslen(msg) * sizeof(wchar_t)), &written, NULL);
    WriteFile(g_hLogFile, L"\r\n", 4, &written, NULL);
    FlushFileBuffers(g_hLogFile);

    LeaveCriticalSection(&g_logCs);
}

// ============================================================================
// WMI Method: Set Thermal Mode via BFn.DoBFn
// ============================================================================
bool WmiSetThermalMode(ThermalMode mode) {
    LogMessage(L"INFO", L"WmiSetThermalMode: mode=%lu", (ULONG)mode);

    // Use STA (Single-Threaded Apartment) to match C# ManagementObject behavior
    HRESULT hr = CoInitializeEx(0, COINIT_APARTMENTTHREADED);
    if (FAILED(hr)) {
        LogMessage(L"ERROR", L"CoInitializeEx failed, hr=0x%08X", hr);
        return false;
    }

    hr = CoInitializeSecurity(
        NULL, -1, NULL, NULL,
        RPC_C_AUTHN_LEVEL_DEFAULT,
        RPC_C_IMP_LEVEL_IMPERSONATE,
        NULL, EOAC_NONE, NULL);
    if (FAILED(hr) && hr != RPC_E_TOO_LATE) {
        LogMessage(L"WARN", L"CoInitializeSecurity failed, hr=0x%08X", hr);
    }

    IWbemLocator* pLoc = NULL;
    hr = CoCreateInstance(CLSID_WbemLocator, 0, CLSCTX_INPROC_SERVER,
                          IID_IWbemLocator, (LPVOID*)&pLoc);
    if (FAILED(hr)) {
        LogMessage(L"ERROR", L"CoCreateInstance(WbemLocator) failed, hr=0x%08X", hr);
        CoUninitialize();
        return false;
    }

    IWbemServices* pSvc = NULL;
    hr = pLoc->ConnectServer(
        _bstr_t(L"ROOT\\WMI"),
        NULL, NULL, 0, NULL, 0, 0, &pSvc);
    pLoc->Release();
    if (FAILED(hr)) {
        LogMessage(L"ERROR", L"ConnectServer(ROOT\\WMI) failed, hr=0x%08X", hr);
        CoUninitialize();
        return false;
    }

    LogMessage(L"INFO", L"Connected to ROOT\\WMI");

    // 1. Create BDat instance
    IWbemClassObject* pBdatClass = NULL;
    hr = pSvc->GetObject(_bstr_t(L"BDat"), 0, NULL, &pBdatClass, NULL);
    if (FAILED(hr) || !pBdatClass) {
        LogMessage(L"ERROR", L"GetObject(BDat) failed, hr=0x%08X", hr);
        pSvc->Release();
        CoUninitialize();
        return false;
    }

    IWbemClassObject* pBdatInst = NULL;
    hr = pBdatClass->SpawnInstance(0, &pBdatInst);
    pBdatClass->Release();
    if (FAILED(hr) || !pBdatInst) {
        LogMessage(L"ERROR", L"SpawnInstance(BDat) failed, hr=0x%08X", hr);
        pSvc->Release();
        CoUninitialize();
        return false;
    }

    // 2. Prepare SmiObject (must match C# BufferLength = 32768)
    static const int BufferLength = 32768;
    static const int MinimumBufferLength = 36;

    DellSmiObject smi = {};
    smi.Class    = 17;      // Class.Info
    smi.Selector = 19;      // Selector.ThermalMode
    smi.Input1   = 1;       // Write operation
    smi.Input2   = (ULONG)mode;

    // Allocate full buffer (32768 bytes) to match C# implementation
    BYTE* buffer = new BYTE[BufferLength];
    ZeroMemory(buffer, BufferLength);
    memcpy(buffer, &smi, sizeof(DellSmiObject));

    VARIANT varBytes;
    VariantInit(&varBytes);
    varBytes.vt = VT_ARRAY | VT_UI1;

    SAFEARRAYBOUND bound;
    bound.lLbound = 0;
    bound.cElements = BufferLength;
    varBytes.parray = SafeArrayCreate(VT_UI1, 1, &bound);
    if (!varBytes.parray) {
        LogMessage(L"ERROR", L"SafeArrayCreate failed");
        delete[] buffer;
        pBdatInst->Release();
        pSvc->Release();
        CoUninitialize();
        return false;
    }

    void* pData = NULL;
    SafeArrayAccessData(varBytes.parray, &pData);
    memcpy(pData, buffer, BufferLength);
    SafeArrayUnaccessData(varBytes.parray);
    delete[] buffer;

    hr = pBdatInst->Put(L"Bytes", 0, &varBytes, 0);
    VariantClear(&varBytes);
    if (FAILED(hr)) {
        LogMessage(L"ERROR", L"Put(Bytes) failed, hr=0x%08X", hr);
        pBdatInst->Release();
        pSvc->Release();
        CoUninitialize();
        return false;
    }

    // 3. Query BFn instances
    IEnumWbemClassObject* pEnum = NULL;
    hr = pSvc->CreateInstanceEnum(_bstr_t(L"BFn"), WBEM_FLAG_FORWARD_ONLY, NULL, &pEnum);
    if (FAILED(hr) || !pEnum) {
        LogMessage(L"ERROR", L"CreateInstanceEnum(BFn) failed, hr=0x%08X", hr);
        pBdatInst->Release();
        pSvc->Release();
        CoUninitialize();
        return false;
    }

    bool success = false;
    IWbemClassObject* pBfn = NULL;
    ULONG uReturn = 0;

    while (pEnum->Next(WBEM_INFINITE, 1, &pBfn, &uReturn) == S_OK && uReturn > 0) {
        VARIANT varName;
        VariantInit(&varName);
        hr = pBfn->Get(L"InstanceName", 0, &varName, 0, 0);

        if (SUCCEEDED(hr) && varName.vt == VT_BSTR && varName.bstrVal) {
            std::wstring name(varName.bstrVal);
            if (_wcsicmp(name.c_str(), L"ACPI\\PNP0C14\\0_0") == 0) {
                LogMessage(L"INFO", L"Found BFn instance: %s", name.c_str());

                // Get the relative object path for ExecMethod
                VARIANT varRelPath;
                VariantInit(&varRelPath);
                hr = pBfn->Get(L"__RELPATH", 0, &varRelPath, 0, 0);
                if (FAILED(hr) || varRelPath.vt != VT_BSTR || !varRelPath.bstrVal) {
                    LogMessage(L"ERROR", L"Get(__RELPATH) failed, hr=0x%08X", hr);
                    VariantClear(&varRelPath);
                    VariantClear(&varName);
                    pBfn->Release();
                    pBfn = NULL;
                    break;
                }
                std::wstring objectPath(varRelPath.bstrVal);
                LogMessage(L"INFO", L"Object path: %s", objectPath.c_str());
                VariantClear(&varRelPath);

                // Create input parameters from __PARAMETERS system class
                // (matches C# ManagementObject.GetMethodParameters behavior)
                IWbemClassObject* pParamClass = NULL;
                hr = pSvc->GetObject(_bstr_t(L"__PARAMETERS"), 0, NULL, &pParamClass, NULL);
                if (FAILED(hr) || !pParamClass) {
                    LogMessage(L"ERROR", L"GetObject(__PARAMETERS) failed, hr=0x%08X", hr);
                    VariantClear(&varName);
                    pBfn->Release();
                    pBfn = NULL;
                    break;
                }

                IWbemClassObject* pInParams = NULL;
                hr = pParamClass->SpawnInstance(0, &pInParams);
                pParamClass->Release();
                if (FAILED(hr) || !pInParams) {
                    LogMessage(L"ERROR", L"SpawnInstance(__PARAMETERS) failed, hr=0x%08X", hr);
                    VariantClear(&varName);
                    pBfn->Release();
                    pBfn = NULL;
                    break;
                }

                // Set Data property to BDat instance
                // Use VT_UNKNOWN with CIM_OBJECT type to match C# ManagementBaseObject behavior
                VARIANT varData;
                VariantInit(&varData);
                varData.vt = VT_UNKNOWN;
                varData.punkVal = pBdatInst;
                pBdatInst->AddRef();

                // CIM_OBJECT = 13 (object reference type)
                hr = pInParams->Put(L"Data", 0, &varData, CIM_OBJECT);
                VariantClear(&varData);

                if (FAILED(hr)) {
                    LogMessage(L"ERROR", L"Put(Data) failed, hr=0x%08X", hr);
                    pInParams->Release();
                    VariantClear(&varName);
                    pBfn->Release();
                    pBfn = NULL;
                    break;
                }

                LogMessage(L"INFO", L"Calling ExecMethod on %s", objectPath.c_str());

                // Execute method using the actual object path
                IWbemClassObject* pOutParams = NULL;
                hr = pSvc->ExecMethod(
                    _bstr_t(objectPath.c_str()),
                    _bstr_t(L"DoBFn"),
                    0, NULL, pInParams, &pOutParams, NULL);

                pInParams->Release();

                if (FAILED(hr)) {
                    LogMessage(L"ERROR", L"ExecMethod(DoBFn) failed, hr=0x%08X", hr);
                    VariantClear(&varName);
                    pBfn->Release();
                    pBfn = NULL;
                    break;
                }

                if (!pOutParams) {
                    LogMessage(L"ERROR", L"ExecMethod returned NULL outParams");
                    VariantClear(&varName);
                    pBfn->Release();
                    pBfn = NULL;
                    break;
                }

                // Get return Data property
                VARIANT varRetData;
                VariantInit(&varRetData);
                hr = pOutParams->Get(L"Data", 0, &varRetData, 0, 0);
                pOutParams->Release();

                if (FAILED(hr)) {
                    LogMessage(L"ERROR", L"Get(Data) from outParams failed, hr=0x%08X", hr);
                    VariantClear(&varName);
                    pBfn->Release();
                    pBfn = NULL;
                    break;
                }

                if (varRetData.vt != VT_UNKNOWN || !varRetData.punkVal) {
                    LogMessage(L"ERROR", L"Return Data is not VT_UNKNOWN, vt=%d", varRetData.vt);
                    VariantClear(&varRetData);
                    VariantClear(&varName);
                    pBfn->Release();
                    pBfn = NULL;
                    break;
                }

                IWbemClassObject* pRetBdat = NULL;
                hr = varRetData.punkVal->QueryInterface(IID_IWbemClassObject, (void**)&pRetBdat);
                VariantClear(&varRetData);

                if (FAILED(hr) || !pRetBdat) {
                    LogMessage(L"ERROR", L"QueryInterface(IID_IWbemClassObject) failed, hr=0x%08X", hr);
                    VariantClear(&varName);
                    pBfn->Release();
                    pBfn = NULL;
                    break;
                }

                VARIANT varRetBytes;
                VariantInit(&varRetBytes);
                hr = pRetBdat->Get(L"Bytes", 0, &varRetBytes, 0, 0);
                pRetBdat->Release();

                if (FAILED(hr)) {
                    LogMessage(L"ERROR", L"Get(Bytes) from return BDat failed, hr=0x%08X", hr);
                    VariantClear(&varName);
                    pBfn->Release();
                    pBfn = NULL;
                    break;
                }

                if ((varRetBytes.vt & VT_ARRAY) == 0 || !varRetBytes.parray) {
                    LogMessage(L"ERROR", L"Return Bytes is not array, vt=%d", varRetBytes.vt);
                    VariantClear(&varRetBytes);
                    VariantClear(&varName);
                    pBfn->Release();
                    pBfn = NULL;
                    break;
                }

                DellSmiObject retSmi = {};
                void* pRetData = NULL;
                SafeArrayAccessData(varRetBytes.parray, &pRetData);
                ULONG arraySize = varRetBytes.parray->rgsabound[0].cElements;
                memcpy(&retSmi, pRetData, min(sizeof(DellSmiObject), arraySize));
                SafeArrayUnaccessData(varRetBytes.parray);
                VariantClear(&varRetBytes);

                LogMessage(L"INFO", L"SMI returned: Output1=%lu, Output2=%lu, Output3=%lu, Output4=%lu",
                           retSmi.Output1, retSmi.Output2, retSmi.Output3, retSmi.Output4);

                if (retSmi.Output1 == 0) {
                    success = true;
                    LogMessage(L"INFO", L"Thermal mode set successfully");
                } else {
                    LogMessage(L"ERROR", L"SMI returned error code: %lu", retSmi.Output1);
                }

                VariantClear(&varName);
                pBfn->Release();
                pBfn = NULL;
                break;
            }
        }
        VariantClear(&varName);
        pBfn->Release();
        pBfn = NULL;
    }

    pEnum->Release();
    pBdatInst->Release();
    pSvc->Release();
    CoUninitialize();

    if (!success) {
        LogMessage(L"ERROR", L"Failed to execute BFn.DoBFn");
    }

    return success;
}

// ============================================================================
// Entry Point
// ============================================================================
int wmain(int argc, wchar_t* argv[]) {
    // Parse mode from command line, default to Quiet (4)
    ULONG modeValue = 4;
    if (argc > 1) {
        modeValue = (ULONG)_wtoi(argv[1]);
    }

    // Validate mode
    if (modeValue != 1 && modeValue != 2 && modeValue != 4 && modeValue != 8) {
        // Invalid mode, write to log but still try with default
        LogInit();
        LogMessage(L"WARN", L"Invalid mode %lu, using default Quiet(4)", modeValue);
        modeValue = 4;
    } else {
        LogInit();
    }

    LogMessage(L"INFO", L"========================================");
    LogMessage(L"INFO", L"DellThermalMode started, target mode=%lu", modeValue);

    bool result = WmiSetThermalMode((ThermalMode)modeValue);

    if (result) {
        LogMessage(L"INFO", L"Thermal mode applied successfully");
    } else {
        LogMessage(L"ERROR", L"Failed to apply thermal mode");
    }

    LogMessage(L"INFO", L"Exiting with code %d", result ? 0 : 1);
    LogClose();

    return result ? 0 : 1;
}
