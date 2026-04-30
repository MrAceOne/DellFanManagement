cmake_minimum_required(VERSION 3.16)
project(DellThermalMode VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Command-line executable (not a service)
add_executable(DellThermalMode DellThermalModeService.cpp)

# Unicode build
target_compile_definitions(DellThermalMode PRIVATE UNICODE _UNICODE)

# Link Windows libraries
target_link_libraries(DellThermalMode PRIVATE
    wbemuuid    # WMI
)

# MSVC specific options
if(MSVC)
    target_compile_options(DellThermalMode PRIVATE /W4 /permissive-)
    # Static runtime to avoid CRT dependency issues
    set_property(TARGET DellThermalMode PROPERTY MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
endif()

# Installation
install(TARGETS DellThermalMode
    RUNTIME DESTINATION bin
)
