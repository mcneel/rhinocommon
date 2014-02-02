#!/bin/bash
set -e

has_openNURBS=false;
has_xcodeProj=false;
has_xcodeTools=true;
has_lipo=true;
is_ready_for_ios_build=false;

has_jni=false;
has_ndk=true;
has_androidmakefile=false;
has_applicationmakefile=false;
is_ready_for_android_build=false;
android_ndk_path="";

did_build_ios_successfully=false;
did_build_android_successfully=false;

function checkAndroid () {
	echo ""
	echo "Android Pre-build check-----------------------------------------"
	openNURBSCheck
	jniCheck
	ndkCheck
	
	if $has_jni && $has_ndk
	then
		is_ready_for_android_build=true;
		echo "STATUS: Ready to build libopennurbs.so for Android"
	fi
	
	if ! $is_ready_for_android_build
	then
		echo "STATUS: NOT ready for Android build.  Please address the following:"
		if ! $has_jni
		then
			echo " ---ERROR: Missing jni folder-------------------------------------------"
			echo "  The jni folder is missing from this copy of openNURBS.  This may be"
			echo "  because this is an older copy of openNURBS or the files have moved."  
			echo "  Go to http://www.rhino3d.com/opennurbs to download and reinstall."
			echo "  NOTE: the jni folder must contain the Android.mk and Application.mk"
			echo "  makefiles that ndk uses to build libopennurbs.so"
		fi
		
		if ! $has_ndk
		then
			echo " ---ERROR: NDK not found------------------------------------------------"
			echo "  Building an android native library requires Google's ndk tools."
			echo "  Xamarin.Android comes with a copy of the Android NDK.  Normally,"
			echo "  this is in /Users/you/Library/Developer/Xamarin/android-ndk/"
			echo "  If you are missing the NKD, you can download a new copy here:"
			echo "  http://developer.android.com/tools/sdk/ndk/index.html"
			echo "  Once installed, you will need to add the path to the NDK toolkit"
			echo "  to your shell profile so that ndk-build can be called from anywhere."
		fi
	fi
}

function checkiOS () {
	echo ""
	echo "iOS Pre-build check---------------------------------------------"
	openNURBSCheck
	xcodeProjCheck
	xcodeToolsCheck
	lipoCheck
	
	if $has_xcodeProj && $has_openNURBS && $has_xcodeTools && $has_lipo
	then
		is_ready_for_ios_build=true;
		echo "STATUS: Ready to build libopennurbs.a for iOS"
	fi
	
	if ! $is_ready_for_ios_build
	then
		echo "STATUS: NOT ready for iOS build.  Please address the following:"
		if ! $has_openNURBS;
		then
			echo " ---ERROR: opennurbs is missing or incomplete---------------------------"
			echo "  Building this library requires openNURBS. Please place a complete"
			echo "  copy of openNURBS in the opennurbs folder before continuing."
			echo "  Go to http://www.rhino3d.com/opennurbs and download the C++"
			echo "  openNURBS SDK and place it in the opennurbs folder contained in"
			echo "  rhinocommon/c/ folder."
		fi
		if ! $has_xcodeProj;
		then
			echo " ---ERROR: Script in wrong location or xcodeproj missing----------------"
			echo "  This script must be placed in the c folder of rhinocommon. This folder"
			echo "  should contain the rhcommon_opennurbs.xcodeproj file that is used in "
			echo "  the command line build.  If this script is in the c folder,"
			echo "  and you are getting this error message, then you are likely missing"
			echo "  the XCode project.  Go to http://github.com/mcneel/rhinocommon"
			echo "  to download and reinstall rhinocommon."
		fi
		
		if ! $has_xcodeTools
		then
			echo " ---ERROR: XCode Command Line Tools Missing-----------------------------"
			echo "  Building the universal binary requires xcodebuild.  This utility"
			echo "  is included with Apple's XCode Command Line Tools.  As of XCode 4.3,"
			echo "  Command Line Tools are optional.  To install Command Line tools, open"
			echo "  XCode > Preferences > Downloads Tab > Components > Command Line Tools."
			echo "  Download and install.  Be sure to close/restart your terminal session"
			echo "  before running this command again."
		fi
		
		if ! $has_lipo
		then
			echo " ---ERROR: Lipo missing or not in PATH----------------------------------"
			echo "  Building the universal binary requires lipo.  If you are seeing this"
			echo "  error, it is likely that lipo is not in your path.  Verify that lipo"
			echo "  is in the /usr/bin folder and that this folder is in your PATH."  
		fi
	fi
}

function jniCheck () {
	printf " Checking for jni                      "
	if test ! -d jni;
	then
		printf "...jni folder NOT FOUND.\n"
		has_jni=false
	fi
	
	if test -d jni;
	then
		cd jni

		if test -f Android.mk;
		then
			has_androidmakefile=true;
		fi

		if test -f Application.mk;
		then
			has_applicationmakefile=true;
		fi

		if $has_androidmakefile && $has_applicationmakefile
		then
			printf "...Ok\n";
			has_jni=true;
		else
			printf "...missing Android.mk or Application.mk\n";
			has_jni=false;
		fi
		
		cd ..
	fi
}

function ndkCheck () {
    printf " Checking for NDK                      "
	#check if ndk-build is already in the path.
	NDKCOMMANDS="ndk-build"
	for i in $NDKCOMMANDS
	do
		command -v $i >/dev/null && continue || { has_ndk=false; }
	done

	if $has_ndk
	then
		printf "...Ok\n"
		has_ndk=true;
		return;
	fi
	
	#check to see if the ndk-build tool is in a typical path and store that in a variable
	if test -d $HOME/Library/Developer/Xamarin/android-ndk;
	then
		if test -f $HOME/Library/Developer/Xamarin/android-ndk/android-ndk-r??/ndk-build;
		then
			has_ndk=true;
			android_ndk_path="$HOME/Library/Developer/Xamarin/android-ndk/android-ndk-r??/"
		else 
			has_ndk=false;
		fi
	fi
	
	if $has_ndk
	then
		printf "...Ok\n"
		has_ndk=true;
	else
    	printf "...ndk NOT FOUND\n"
	fi
}

function openNURBSCheck () {
	printf " Checking for opennurbs                "
	if test ! -d opennurbs/opennurbs.xcodeproj;
	then
	  printf "...opennurbs NOT FOUND\n"
	  has_openNURBS=false
	fi
	
	if test -d opennurbs/opennurbs.xcodeproj;
	then
		printf "...Ok\n"
		has_openNURBS=true
	fi
}

function xcodeProjCheck () {
	printf " Checking for xcodeproj                "
	if test ! -d rhcommon_opennurbs.xcodeproj;
	then
	  printf "...rhcommon_opennurbs.xcodeproj NOT FOUND\n"
	  has_xcodeProj=false
	fi
	
	if test -d rhcommon_opennurbs.xcodeproj;
	then
		printf "...Ok\n"
		has_xcodeProj=true
	fi
}

function xcodeToolsCheck () {
    printf " Checking for XCode command line tools "
	XCCOMMANDS="xcodebuild"
	for i in $XCCOMMANDS
	do
		command -v $i >/dev/null && continue || { printf "...$i NOT FOUND\n"; has_xcodeTools=false; }
	done
	
	if $has_xcodeTools
	then
		printf "...Ok\n"
		has_xcodeTools=true;
	fi
}

function lipoCheck () {
	printf " Checking for lipo                     "
	LIPOCOMMANDS="lipo"
	for i in $LIPOCOMMANDS
	do
		command -v $i >/dev/null && continue || { printf "...$i NOT FOUND.\n"; has_lipo=false;}
	done

	if $has_lipo
	then
		printf "...Ok\n"
	fi
}

function createBuildFoldersForIOS () {
	if test ! -d build
	then
		mkdir build
	fi
	
	#check to make sure the folder was created successfully
	if test ! -d build
	then
		echo "ERROR: Unable to create build folders.  Please make sure you have admin privileges and try again."
		exit 1;
	fi
	
	cd build
	
	if test ! -d Release-ios
	then
		mkdir Release-ios
	fi
	
	#check to make sure the folder was created successfully
	if test ! -d Release-ios
	then
		echo "ERROR: Unable to create build folders.  Please make sure you have admin privileges and try again."
		exit 1;
	fi
	
	cd ..
}

function createBuildFoldersForAndroid() {
	if test ! -d build
	then
		mkdir build
	fi
	
	#check to make sure the folder was created successfully
	if test ! -d build
	then
		echo "ERROR: Unable to create build folders.  Please make sure you have admin privileges and try again."
		exit 1;
	fi
	
	cd build
	
	if test ! -d Release-android
	then
		mkdir Release-android
	fi
	
	#check to make sure the folder was created successfully
	if test ! -d Release-android
	then
		echo "ERROR: Unable to create build folders.  Please make sure you have admin privileges and try again."
		exit 1;
	fi
	
	cd ..
}

function buildForAndroid() {
	echo ""
	echo "Android Build---------------------------------------------------"
	echo "WARNING: go get coffee, this can take 20 minutes."
	echo "Making libopennurbs.so for Android..."

	ndk_command="ndk-build"
	ndk_build=$android_ndk_path$ndk_command
	$ndk_build
	
	if test -d build/Release-android/libs;
	then
		rm -rf build/Release-android/libs
	fi
	
	mv libs build/Release-android/
	
	#cleanup objects
	rm -rf obj
	
	did_build_android_successfully=true;
}

function buildForiOS() {
	echo ""
	echo "iOS Build-------------------------------------------------------"
	echo "Making static libopennurbs.a for iOS..."
	
	printf " Compiling i386 version                "
	# add: > /dev/null  to suppress output on xcodeBuilds
	xcodeBuild -project rhcommon_opennurbs.xcodeproj -target opennurbs -sdk iphonesimulator -configuration Release clean build
	printf "...Done\n";
	cd build/Release-iphonesimulator
	mv libopennurbs.a libopennurbs-i386.a
	mv libopennurbs-i386.a ../../build/Release-ios
	cd ../..

	printf " Compiling armv7 version               "
	xcodebuild -project rhcommon_opennurbs.xcodeproj -target opennurbs -sdk iphoneos -arch armv7 -configuration Release clean build
	printf "...Done\n";
	cd build/Release-iphoneos
	mv libopennurbs.a libopennurbs-armv7.a
	mv libopennurbs-armv7.a ../../build/Release-ios
	cd ../..
	
	printf " Compiling armv7s version              "
	xcodebuild -project rhcommon_opennurbs.xcodeproj -target opennurbs -sdk iphoneos -arch armv7s -configuration Release clean build
	printf "...Done\n";
	cd build/Release-iphoneos
	mv libopennurbs.a libopennurbs-armv7s.a
	mv libopennurbs-armv7s.a ../../build/Release-ios
	cd ../..
	
	printf " Creating Universal Binary             "
	cd build/Release-ios
	lipo -create -output libopennurbs.a libopennurbs-i386.a libopennurbs-armv7.a libopennurbs-armv7s.a
	printf "...Done\n";
	cd ../..
	
	did_build_ios_successfully=true;
}

function androidFinishedMessage () {
	echo "STATUS: Android Build Complete.  Libraries are in build/Release-android/libs"
}

function iOSFinishedMessage () {
	echo "STATUS: iOS Build Complete.  Libraries are in build/Release-ios"
}

function help () {
	echo ""
    echo "build_mobile.sh - bash shell script for building openNURBS for mobile platforms."
    echo "usage: ./build_mobile.sh argument"
    echo ""
    echo "    argument:   description:"
    echo ""
    echo "    all         build libopennurbs for all mobile platforms" 
    echo "    android     build libopennurbs.so for Android"
	echo "    check       perform pre-requisites check for each platform"
	echo "    help        display this screen"
    echo "    ios         build libopennurbs.a for iOS"
	echo ""
	echo "NOTE: Successful builds are stored in the build/[platform]/ folder."
	echo ""
}

if [ "$1" = "help" ] || [ "$1" = "?" ] || [ "$1" = "" ]
then
    help
elif [ "$1" = "check" ]
then
	checkAndroid
	checkiOS
elif [ "$1" = "android" ]
then
	checkAndroid
	createBuildFoldersForAndroid
	if $is_ready_for_android_build;
	then
		buildForAndroid
	fi

	if $did_build_android_successfully;
	then
		androidFinishedMessage
	fi
elif [ "$1" = "ios" ]
then
	checkiOS
	createBuildFoldersForIOS
	if $is_ready_for_ios_build;
	then
		buildForiOS
	fi
	
	if $did_build_ios_successfully;
	then
		iOSFinishedMessage
	fi
elif [ "$1" = "all" ]
then
	checkAndroid
	createBuildFoldersForAndroid
	checkiOS
	createBuildFoldersForIOS
	
	if $is_ready_for_android_build;
	then
		buildForAndroid
	fi
	
	if $is_ready_for_ios_build;
	then
		buildForiOS
	fi
	
	if $did_build_android_successfully;
	then
		androidFinishedMessage
	fi
	
	if $did_build_ios_successfully;
	then
		iOSFinishedMessage
	fi
else
    echo "ERROR: unknown argument.  build_mobile.sh help"
    help
fi

exit 0