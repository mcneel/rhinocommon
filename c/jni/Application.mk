# This is the Application NDK make file for RhinoCommon shared library for Android.
# dan@mcneel.com
# Last Revised: 10/07/13

# Build for Android API level 14
APP_PLATFORM := android-14

# Build Intel, ARMv5TE and ARMv7-A machine code.
APP_ABI := armeabi armeabi-v7a x86

# Load the modules...at this point, only opennurbs, no opennurbs-prebuilt
APP_MODULES := opennurbs

# Tell NDK to place its compiler flags before the local compiler flags in Android.mk
#APP_CFLAGS += -Ofast
#APP_CPPFLAGS += -Ofast