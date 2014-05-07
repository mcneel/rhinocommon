#!/usr/bin/env python
import subprocess
import sys
import getopt
import os
import glob
import logging
import shutil
from sys import platform as _platform
from os import listdir
from os.path import isfile, isdir, join

verbose = False
overwrite = False
has_built_ios = False
has_built_android = False

#openNURBS globals
has_openNURBS = False

#iOS globals
has_xcodeProj = False
has_xcodeTools = False
has_lipo = False
is_ready_for_ios_build = False
did_build_ios_successfully = False

#Android globals
has_jni = False
has_ndk = False
has_android_makefile = False
has_application_makefile = False
is_ready_for_android_build = False
android_ndk_path = ''
did_build_android_successfully = False


def check_android():
    print ""
    print "Android Pre-build check-----------------------------------------"
    check_opennurbs()
    check_jni()
    check_ndk()
    check_has_built_for_android()

    global is_ready_for_android_build

    if has_jni and has_ndk:
        is_ready_for_android_build = True
        print "STATUS: Ready to build libopennurbs.so for Android"
    else:
        is_ready_for_android_build = False

    if not is_ready_for_android_build:
        print "STATUS: NOT ready for Android build.  Please address the following:"
        if not has_jni:
            print " ---ERROR: Missing jni folder-------------------------------------------"
            print "  The jni folder is missing from this copy of openNURBS.  This may be"
            print "  because this is an older copy of openNURBS or the files have moved."
            print "  Go to http://www.rhino3d.com/opennurbs to download and reinstall."
            print "  NOTE: the jni folder must contain the Android.mk and Application.mk"
            print "  makefiles that ndk uses to build libopennurbs.so"

        if not has_ndk:
            print " ---ERROR: NDK not found------------------------------------------------"
            print "  Building an android native library requires Google's ndk tools."
            print "  Xamarin.Android comes with a copy of the Android NDK.  Normally,"
            print "  this is in /Users/you/Library/Developer/Xamarin/android-ndk/"
            print "  If you are missing the NKD, you can download a new copy here:"
            print "  http://developer.android.com/tools/sdk/ndk/index.html"
            print "  Once installed, you will need to add the path to the NDK toolkit"
            print "  to your shell profile so that ndk-build can be called from anywhere."
    return


def check_ios():
    print ""
    print "iOS Pre-build check---------------------------------------------"
    check_opennurbs()
    check_xcodeproj()
    check_xcode_tools()
    check_lipo()
    check_has_built_for_ios()

    global is_ready_for_ios_build

    if has_xcodeProj and has_openNURBS and has_xcodeTools and has_lipo:
        is_ready_for_ios_build = True
        print "STATUS: Ready to build libopennurbs.a for iOS"
    else:
        is_ready_for_ios_build = False

    if not is_ready_for_ios_build:
        print "STATUS: NOT ready for iOS build.  Please address the following:"
        if not has_openNURBS:
            print " ---ERROR: opennurbs is missing or incomplete---------------------------"
            print "  Building this library requires openNURBS. Please place a complete"
            print "  copy of openNURBS in the opennurbs folder before continuing."
            print "  Go to http://www.rhino3d.com/opennurbs and download the C++"
            print "  openNURBS SDK and place it in the opennurbs folder contained in"
            print "  rhinocommon/c/ folder."

        if not has_xcodeProj:
            print " ---ERROR: Script in wrong location or xcodeproj missing----------------"
            print "  This script must be placed in the c folder of rhinocommon. This folder"
            print "  should contain the rhcommon_opennurbs.xcodeproj file that is used in "
            print "  the command line build.  If this script is in the c folder,"
            print "  and you are getting this error message, then you are likely missing"
            print "  the XCode project.  Go to http://github.com/mcneel/rhinocommon"
            print "  to download and reinstall rhinocommon."

        if not has_xcodeTools:
            print " ---ERROR: XCode Command Line Tools Missing-----------------------------"
            print "  Building the universal binary requires xcodebuild.  This utility"
            print "  is included with Apple's XCode Command Line Tools.  As of XCode 4.3,"
            print "  Command Line Tools are optional.  To install Command Line tools, open"
            print "  XCode > Preferences > Downloads Tab > Components > Command Line Tools."
            print "  Download and install.  Be sure to close/restart your terminal session"
            print "  before running this command again."

        if not has_lipo:
            print " ---ERROR: Lipo missing or not in PATH----------------------------------"
            print "  Building the universal binary requires lipo.  If you are seeing this"
            print "  error, it is likely that lipo is not in your path.  Verify that lipo"
            print "  is in the /usr/bin folder and that this folder is in your PATH."
    return


def check_jni():
    sys.stdout.write(" Checking for jni                      ")
    global has_jni
    global has_android_makefile
    global has_application_makefile
    if not os.path.exists("jni"):
        sys.stdout.write("...jni folder NOT FOUND.\n")
        has_jni = False

    if os.path.exists("jni"):
        if os.path.exists("jni/Android.mk"):
            has_android_makefile = True
        if os.path.exists("jni/Application.mk"):
            has_application_makefile = True
        if has_android_makefile and has_application_makefile:
            sys.stdout.write("...Found\n")
            has_jni = True
        else:
            sys.stdout.write("...missing Android.mk or Application.mk\n")
            has_jni = False


def check_ndk():
    sys.stdout.write(" Checking for NDK                      ")

    global has_ndk

    #check if ndk-build is already in the path.
    is_executable = which("ndk-build")

    if is_executable is None:
        has_ndk = False
    else:
        has_ndk = True

    if has_ndk:
        sys.stdout.write("...Found\n")
        has_ndk = True
        return

    #check to see if the ndk-build tool is in a typical path and store that in a variable
    home = os.path.expanduser("~")
    if os.path.exists(home + '/Library/Developer/Xamarin/android-ndk'):
        if glob.glob(home + '/Library/Developer/Xamarin/android-ndk/android-ndk-r??/ndk-build'):
            has_ndk = True
            path_to_search = home + '/Library/Developer/Xamarin/android-ndk/'

            only_folders = [d for d in listdir(path_to_search) if isdir(join(path_to_search, d))]
            most_recent_ndk = only_folders[-1]

            global android_ndk_path
            android_ndk_path = home + '/Library/Developer/Xamarin/android-ndk/' + most_recent_ndk + '/'
        else:
            has_ndk = False

    if has_ndk:
        sys.stdout.write("...Found\n")
    else:
        sys.stdout.write("...ndk NOT FOUND\n")


def check_opennurbs():
    sys.stdout.write(" Checking for opennurbs                ")
    global has_openNURBS
    if os.path.exists("opennurbs/opennurbs.xcodeproj"):
        sys.stdout.write("...Found\n")
        has_openNURBS = True
    else:
        sys.stdout.write("...opennurbs NOT FOUND\n")
        has_openNURBS = False


def check_xcodeproj():
    sys.stdout.write(" Checking for xcodeproj                ")
    global has_xcodeProj
    if os.path.exists("rhcommon_opennurbs.xcodeproj"):
        sys.stdout.write("...Found\n")
        has_xcodeProj = True
    else:
        sys.stdout.write("...rhcommon_opennurbs.xcodeproj NOT FOUND\n")
        has_xcodeProj = False


def check_xcode_tools():
    sys.stdout.write(" Checking for XCode command line tools ")
    global has_xcodeTools

    if which("xcodebuild") is None:
        sys.stdout.write("...xcodebuild NOT FOUND\n")
        has_xcodeTools = False
    else:
        sys.stdout.write("...Found\n")
        has_xcodeTools = True


def check_lipo():
    sys.stdout.write(" Checking for lipo                     ")
    global has_lipo

    if which("lipo") is None:
        sys.stdout.write("...lipo NOT FOUND\n")
        has_lipo = False
    else:
        sys.stdout.write("...Found\n")
        has_lipo = True


def check_has_built_for_android():
    sys.stdout.write(" Checking for existing builds          ")
    global has_built_android
    if os.path.exists("build/Release-android/libs/armeabi/libopennurbs.so") and os.path.exists("build/Release-android/libs/armeabi-v7a/libopennurbs.so") and os.path.exists("build/Release-android/libs/x86/libopennurbs.so"):
        sys.stdout.write("...Found\n")
        has_built_android = True
    else:
        sys.stdout.write("...Not Found\n")
        has_built_android = False


def check_has_built_for_ios():
    sys.stdout.write(" Checking for existing builds          ")
    global has_built_ios
    if os.path.exists("build/Release-ios/libopennurbs.a"):
        sys.stdout.write("...Found\n")
        has_built_ios = True
    else:
        sys.stdout.write("...Not Found\n")
        has_built_ios = False


def create_build_folders_for_ios():
    if not os.path.exists("build"):
        os.mkdir("build")

    #check to make sure the folder was created successfully
    if not os.path.exists("build"):
        print "ERROR: Unable to create build folders.  Please make sure you have admin privileges and try again."
        sys.exit()

    if not os.path.exists("build/Release-ios"):
        os.mkdir("build/Release-ios")

    #check to make sure the folder was created successfully
    if not os.path.exists("build/Release-ios"):
        print "ERROR: Unable to create build folders.  Please make sure you have admin privileges and try again."
        sys.exit()


def create_build_folders_for_android():
    if not os.path.exists("build"):
        os.mkdir("build")

    #check to make sure the folder was created successfully
    if not os.path.exists("build"):
        print "ERROR: Unable to create build folders.  Please make sure you have admin privileges and try again."
        sys.exit()

    if not os.path.exists("build/Release-android"):
        os.mkdir("build/Release-android")

    #check to make sure the folder was created successfully
    if not os.path.exists("build/Release-android"):
        print "ERROR: Unable to create build folders.  Please make sure you have admin privileges and try again."
        sys.exit()


def build_for_android():
    print ""
    print "Android Build---------------------------------------------------"
    print "WARNING: go get coffee, this can take 20 minutes."
    print "Making libopennurbs.so for Android..."
    ndk_command = "ndk-build"
    ndk_build = android_ndk_path + ndk_command

    if verbose:
        subprocess.call(ndk_build)
    else:
        devnull = open(os.devnull, 'w')
        subprocess.call(ndk_build, stdout=devnull, stderr=devnull)

    #cleanup any old builds...
    if os.path.exists("build/Release-android/libs"):
        shutil.rmtree("build/Release-android/libs")

    #move the new build into the proper location...
    if os.path.exists("libs"):
        shutil.move("libs", "build/Release-android")

    #cleanup objects...
    if os.path.exists("obj"):
        shutil.rmtree("obj")

    global did_build_android_successfully
    did_build_android_successfully = True


def build_for_ios():
    print ""
    print "iOS Build-------------------------------------------------------"
    print "Making static libopennurbs.a for iOS..."

    sys.stdout.write(" Compiling i386 version                ")
    if verbose:
        subprocess.call(["xcodeBuild", "-project", "rhcommon_opennurbs.xcodeproj", "-target", "opennurbs", "-sdk", "iphonesimulator", "-configuration", "Release", "clean", "build"])
    else:
        devnull = open(os.devnull, 'w')
        subprocess.call(["xcodeBuild", "-project", "rhcommon_opennurbs.xcodeproj", "-target", "opennurbs", "-sdk", "iphonesimulator", "-configuration", "Release", "clean", "build"], stdout=devnull, stderr=devnull)

    if os.path.exists("build/Release-iphonesimulator/libopennurbs.a"):
        shutil.move("build/Release-iphonesimulator/libopennurbs.a", "build/Release-ios/libopennurbs-i386.a")
        sys.stdout.write("...Done\n")
    else:
        sys.stdout.write("...FAILED\n")
        sys.exit()

    sys.stdout.write(" Compiling armv7 version               ")
    if verbose:
        subprocess.call(["xcodeBuild", "-project", "rhcommon_opennurbs.xcodeproj", "-target", "opennurbs", "-sdk", "iphoneos", "-arch", "armv7", "-configuration", "Release", "clean", "build"])
    else:
        devnull = open(os.devnull, 'w')
        subprocess.call(["xcodeBuild", "-project", "rhcommon_opennurbs.xcodeproj", "-target", "opennurbs", "-sdk", "iphoneos", "-arch", "armv7", "-configuration", "Release", "clean", "build"], stdout=devnull, stderr=devnull)

    if os.path.exists("build/Release-iphoneos/libopennurbs.a"):
        shutil.move("build/Release-iphoneos/libopennurbs.a", "build/Release-ios/libopennurbs-armv7.a")
        sys.stdout.write("...Done\n")
    else:
        sys.stdout.write("...FAILED\n")
        sys.exit()

    sys.stdout.write(" Compiling armv7s version              ")
    if verbose:
        subprocess.call(["xcodeBuild", "-project", "rhcommon_opennurbs.xcodeproj", "-target", "opennurbs", "-sdk", "iphoneos", "-arch", "armv7s", "-configuration", "Release", "clean", "build"])
    else:
        devnull = open(os.devnull, 'w')
        subprocess.call(["xcodeBuild", "-project", "rhcommon_opennurbs.xcodeproj", "-target", "opennurbs", "-sdk", "iphoneos", "-arch", "armv7s", "-configuration", "Release", "clean", "build"], stdout=devnull, stderr=devnull)

    if os.path.exists("build/Release-iphoneos/libopennurbs.a"):
        shutil.move("build/Release-iphoneos/libopennurbs.a", "build/Release-ios/libopennurbs-armv7s.a")
        sys.stdout.write("...Done\n")
    else:
        sys.stdout.write("...FAILED\n")
        sys.exit()

    sys.stdout.write(" Creating Universal Binary             ")
    if verbose:
        subprocess.call(["lipo", "-create", "-output", "build/Release-ios/libopennurbs.a", "build/Release-ios/libopennurbs-i386.a", "build/Release-ios/libopennurbs-armv7.a", "build/Release-ios/libopennurbs-armv7s.a"])
    else:
        devnull = open(os.devnull, 'w')
        subprocess.call(["lipo", "-create", "-output", "build/Release-ios/libopennurbs.a", "build/Release-ios/libopennurbs-i386.a", "build/Release-ios/libopennurbs-armv7.a", "build/Release-ios/libopennurbs-armv7s.a"], stdout=devnull, stderr=devnull)

    sys.stdout.write("...Done\n")

    global did_build_ios_successfully
    did_build_ios_successfully = True


def write_android_finished_message():
    print "STATUS: Android Build Complete.  Libraries are in build/Release-android/libs"


def write_ios_finished_message():
    print "STATUS: iOS Build Complete.  Libraries are in build/Release-ios"


def which(program):
    def is_exe(fpath):
        return os.path.isfile(fpath) and os.access(fpath, os.X_OK)

    fpath, fname = os.path.split(program)
    if fpath:
        if is_exe(program):
            return program
    else:
        for path in os.environ["PATH"].split(os.pathsep):
            path = path.strip('"')
            exe_file = os.path.join(path, program)
            if is_exe(exe_file):
                return exe_file

    return None


def usage():
    print ""
    print("build_mobile.py - python script for building openNURBS for mobile platforms.")
    print "usage: ./build_mobile.py -p [argument] --check --help --verbose"
    print ""
    print "    option:              arguments:      description:"
    print "    ---------------      ----------      ----------------------------------------------"
    print "    -p   --platform      all             build libopennurbs for all mobile platforms"
    print "                         android         build libopennurbs.so for Android"
    print "                         ios             build libopennurbs.so for iOS"
    print "    -c   --check         all             perform pre-requisites check for all platforms"
    print "                         android         perform pre-requisites check for Android"
    print "                         ios             perform pre-requisites check for iOS"
    print "    -h   --help                          display this screen"
    print "    -v   --verbose                       show verbose build messages"
    print "    -o   --overwrite                     overwrite existing builds"
    print ""
    print "Examples:"
    print "./build_mobile.py -p all         (build libopennurbs for all mobile platforms)"
    print "./build_mobile.py -p ios -v      (build libopennurbs for all iOS with verbose messages)"
    print "./build_mobile.py -c android     (check pre-requisites for Android)"
    print "./build_mobile.py -p android -o  (build libopennurbs for android overwriting existing builds)"
    print ""
    print "NOTE: Builds are stored in the build/[platform]/ folder."
    print ""
    return


def main():
    try:
        opts, args = getopt.getopt(sys.argv[1:], "hvoc:p:", ["help", "verbose", "overwrite", "check=", "platform="])
    except getopt.GetoptError as err:
        # print help information and exit:
        usage()
        sys.exit(2)
    platform = None
    check = None
    linux = False
    osx = False
    windows = False

    #check os
    if _platform == "linux" or _platform == "linux2":
        linux = True
    elif _platform == "darwin":
        osx = True
    elif _platform == "win32" or _platform == "cygwin":
        windows = True

    for o, a in opts:
        if o == "-v" or o == "--verbose":
            global verbose
            verbose = True
        elif o == "-o" or o == "--overwrite":
            global overwrite
            overwrite = True
        elif o in ("-h", "--help"):
            usage()
            sys.exit()
        elif o in ("-p", "--platform"):
            platform = a
        elif o in ("-c", "--check"):
            check = a
        else:
            assert False, "unhandled option"

    #user has not entered any arguments...
    if platform is None and check is None:
        usage()
        sys.exit()

    if not osx:
        print ("WARNING: Building for iOS requires running this script on Mac OSX 10.9.2 or higher.")

    #check prerequisites
    if check == "all":
        check_android()
        check_ios()
    elif check == "android":
        check_android()
    elif check == "ios":
        check_ios()

    #platform compiles
    if platform == "all":
        check_android()
        check_ios()

        if overwrite or not has_built_android:
            create_build_folders_for_android()
        elif has_built_android and not overwrite:
            print ("STATUS: Existing Android build found.  NOT BUILDING.  (Use -o argument to overwrite existing.)")

        if overwrite or not has_built_ios:
            create_build_folders_for_ios()
        elif has_built_ios and not overwrite:
            print ("STATUS: Existing iOS build found.  NOT BUILDING.  (Use -o argument to overwrite existing.)")

        if overwrite and is_ready_for_android_build:
            build_for_android()
        elif not has_built_android and is_ready_for_android_build:
            build_for_android()

        if overwrite and is_ready_for_ios_build:
            build_for_ios()
        elif not has_built_ios and is_ready_for_ios_build:
            build_for_ios()

        if did_build_android_successfully:
            write_android_finished_message()

        if did_build_ios_successfully:
            write_ios_finished_message()
    elif platform == "android":
        check_android()

        if has_built_android and not overwrite:
            print ("STATUS: Existing build found.  NOT BUILDING.  Use -o argument to overwrite the existing builds")
            sys.exit()

        if overwrite or not has_built_android:
            create_build_folders_for_android()

            if is_ready_for_android_build:
                build_for_android()
            else:
                sys.exit()

            if did_build_android_successfully:
                write_android_finished_message()
    elif platform == "ios":
        check_ios()

        if has_built_ios and not overwrite:
            print ("STATUS: Existing build found.  NOT BUILDING.  Use -o argument to overwrite the existing builds")
            sys.exit()

        if overwrite or not has_built_ios:
            create_build_folders_for_ios()

            if is_ready_for_ios_build:
                build_for_ios()
            else:
                sys.exit()

            if did_build_ios_successfully:
                write_ios_finished_message()


if __name__ == "__main__":
    main()