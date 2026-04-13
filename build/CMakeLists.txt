cmake_minimum_required(VERSION 3.21)
project(libremidi_native VERSION 0.1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Enable libremidi's C API
set(LIBREMIDI_C_API ON CACHE BOOL "Build the libremidi C API" FORCE)
set(LIBREMIDI_HEADER_ONLY OFF CACHE BOOL "" FORCE)
set(BUILD_SHARED_LIBS ON)

add_subdirectory(${CMAKE_CURRENT_SOURCE_DIR}/../external/libremidi
                 ${CMAKE_CURRENT_BINARY_DIR}/libremidi)

# The C API target is exposed as 'libremidi' by libremidi's own CMakeLists.
# Output goes to the runtimes/<rid>/native/ directory set by the caller via
# CMAKE_RUNTIME_OUTPUT_DIRECTORY / CMAKE_LIBRARY_OUTPUT_DIRECTORY.
set_target_properties(libremidi PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${OUTPUT_DIR}"   # .dll on Windows
    LIBRARY_OUTPUT_DIRECTORY "${OUTPUT_DIR}"   # .so/.dylib on Unix
)
