###############################################################

# If your make doesn't use .cpp for C++ extensions,
# then uncomment the suffixes line.
.SUFFIXES : .c .cpp

RM = /bin/rm
# If you don't mind the risk of using the -f option and your rm
# asks too many questions, then uncomment the next line.
# RM = /bin/rm -f

MKDIR = /bin/mkdir -p
CP = /bin/cp
AR = ar qvl

# If your system doesn't use ranlib, uncomment the "echo" define.
RANLIB = ranlib
# RANLIB = echo

###############################################################
# Gnu tools
#
# (Tested with gcc 4.2.1 on OSX 10.8.1)
#
# In order to get full wide character (UNICODE) support, you
# need to define _GNU_SOURCE

# Choose one of the following definitions for GCC_OPTIMIZER_FLAGS

# unoptimized debug build
ON_GNU_OPTIMIZER_FLAGS = -g

# optimized release build
#ON_GNU_OPTIMIZER_FLAGS = -O3

# C compiler and flags
CC = gcc
CFLAGS = $(ON_GNU_OPTIMIZER_FLAGS) -Wall -shared -fPIC -D__LINUX__ -DOPENNURBS_BUILD -D_GNU_SOURCE -DMY_ZCALLOC -DZ_PREFIX -I.

# C++ compiler and flags
CCC = g++
CCFLAGS = $(ON_GNU_OPTIMIZER_FLAGS) -Wall -shared -fPIC -D__LINUX__ -DOPENNURBS_BUILD -D_GNU_SOURCE -I.

CCCSHARED = $(CCC) -Wall -shared -fPIC

LINK = $(CCC)
LINKFLAGS =

###############################################################
# Irix 6.5 compiler (uncomment the lines in this block)
#
#

#OPTIMIZER       = -g
#OPTIMIZER       = -O3

#ABIISA          = -n32 -mips3
#ABIISA          = -64  -mips3

#IRIXDEF 	= -D_WCHAR_T_DEFINED -DON_COMPILER_IRIX -D_GNU_SOURCE

#IRIXFLAGS 	= -woff all -no_prelink -ptused

#CC = cc
#CFLAGS = $(OPTIMIZER) $(ABIISA) $(IRIXDEF) $(IRIXFLAGS) -DMY_ZCALLOC -DZ_PREFIX -I.

#CCC = CC
#CCFLAGS = $(OPTIMIZER) $(ABIISA) $(IRIXDEF) $(IRIXFLAGS) -I.

#LINK = $(CCC)
#LINKFLAGS =
# uncomment for 64 bit linking
#LINKFLAGS = $CCFLAGS

#
###############################################################





###############################################################

.cpp.o :
	$(CCC) $(CCFLAGS) -c $*.cpp -o $*.o

# If your make doesn't have a rule for .c to .o, uncomment the
# next two lines.
#.c.o :
#	$(CC) $(CFLAGS) -c $*.c -o $*.o


###############################################################

OPENNURBS_LIB_NAME = openNURBS
OPENNURBS_LIB_FILE = lib$(OPENNURBS_LIB_NAME).a
OPENNURBS_SHARED_LIB_FILE = librhino3dmio_native.so

ON_INC= 


ON_SRC= on_3dm_attributes.cpp \
		on_3dm_settings.cpp \
		on_annotation2.cpp \
		on_arc.cpp \
		on_arccurve.cpp \
		on_archive.cpp \
		on_array.cpp \
		on_beam.cpp \
		on_bezier.cpp \
		on_brep.cpp \
		on_circle.cpp \
		on_curve.cpp \
		on_defines.cpp \
		on_detail.cpp \
		on_dimstyle.cpp \
		on_geometry.cpp \
		on_hatch.cpp \
		on_instance.cpp \
		on_intersect.cpp \
		on_layer.cpp \
		on_light.cpp \
		on_line.cpp \
		on_linecurve.cpp \
		on_linetype.cpp \
		on_massprop.cpp \
		on_material.cpp \
		on_mesh.cpp \
		on_nurbscurve.cpp \
		on_nurbssurface.cpp \
		on_object.cpp \
		on_plane.cpp \
		on_planesurface.cpp \
		on_plus.cpp \
		on_point.cpp \
		on_pointcloud.cpp \
		on_pointgeometry.cpp \
		on_pointgrid.cpp \
		on_polycurve.cpp \
		on_polylinecurve.cpp \
		on_quaternion.cpp \
		on_rtree.cpp \
		on_sphere.cpp \
		on_surface.cpp \
		on_textlog.cpp \
		on_userdata.cpp \
		on_viewport.cpp \
		on_xform.cpp \
		stringholder.cpp \
		stdafx.cpp \
		opennurbs/opennurbs_3dm_attributes.cpp \
		opennurbs/opennurbs_3dm_properties.cpp \
		opennurbs/opennurbs_3dm_settings.cpp \
		opennurbs/opennurbs_annotation.cpp \
		opennurbs/opennurbs_annotation2.cpp \
		opennurbs/opennurbs_arc.cpp \
		opennurbs/opennurbs_arccurve.cpp \
		opennurbs/opennurbs_archive.cpp \
		opennurbs/opennurbs_array.cpp \
		opennurbs/opennurbs_base32.cpp \
		opennurbs/opennurbs_base64.cpp \
		opennurbs/opennurbs_beam.cpp \
		opennurbs/opennurbs_bezier.cpp \
		opennurbs/opennurbs_beziervolume.cpp \
		opennurbs/opennurbs_bitmap.cpp \
		opennurbs/opennurbs_bounding_box.cpp \
		opennurbs/opennurbs_box.cpp \
		opennurbs/opennurbs_brep.cpp \
		opennurbs/opennurbs_brep_extrude.cpp \
		opennurbs/opennurbs_brep_io.cpp \
		opennurbs/opennurbs_brep_isvalid.cpp \
		opennurbs/opennurbs_brep_region.cpp \
		opennurbs/opennurbs_brep_tools.cpp \
		opennurbs/opennurbs_brep_v2valid.cpp \
		opennurbs/opennurbs_circle.cpp \
		opennurbs/opennurbs_color.cpp \
		opennurbs/opennurbs_compress.cpp \
		opennurbs/opennurbs_cone.cpp \
		opennurbs/opennurbs_crc.cpp \
		opennurbs/opennurbs_curve.cpp \
		opennurbs/opennurbs_curveonsurface.cpp \
		opennurbs/opennurbs_curveproxy.cpp \
		opennurbs/opennurbs_cylinder.cpp \
		opennurbs/opennurbs_defines.cpp \
		opennurbs/opennurbs_detail.cpp \
		opennurbs/opennurbs_dimstyle.cpp \
		opennurbs/opennurbs_ellipse.cpp \
		opennurbs/opennurbs_embedded_file.cpp \
		opennurbs/opennurbs_error.cpp \
		opennurbs/opennurbs_error_message.cpp \
		opennurbs/opennurbs_evaluate_nurbs.cpp \
		opennurbs/opennurbs_extensions.cpp \
		opennurbs/opennurbs_font.cpp \
		opennurbs/opennurbs_fsp.cpp \
		opennurbs/opennurbs_geometry.cpp \
		opennurbs/opennurbs_group.cpp \
		opennurbs/opennurbs_hatch.cpp \
		opennurbs/opennurbs_instance.cpp \
		opennurbs/opennurbs_intersect.cpp \
		opennurbs/opennurbs_knot.cpp \
		opennurbs/opennurbs_layer.cpp \
		opennurbs/opennurbs_light.cpp \
		opennurbs/opennurbs_line.cpp \
		opennurbs/opennurbs_linecurve.cpp \
		opennurbs/opennurbs_linetype.cpp \
		opennurbs/opennurbs_lookup.cpp \
		opennurbs/opennurbs_material.cpp \
		opennurbs/opennurbs_math.cpp \
		opennurbs/opennurbs_matrix.cpp \
		opennurbs/opennurbs_memory.c \
		opennurbs/opennurbs_memory_util.c \
		opennurbs/opennurbs_mesh.cpp \
		opennurbs/opennurbs_mesh_ngon.cpp \
		opennurbs/opennurbs_mesh_tools.cpp \
		opennurbs/opennurbs_morph.cpp \
		opennurbs/opennurbs_nurbscurve.cpp \
		opennurbs/opennurbs_nurbssurface.cpp \
		opennurbs/opennurbs_nurbsvolume.cpp \
		opennurbs/opennurbs_object.cpp \
		opennurbs/opennurbs_object_history.cpp \
		opennurbs/opennurbs_objref.cpp \
		opennurbs/opennurbs_offsetsurface.cpp \
		opennurbs/opennurbs_optimize.cpp \
		opennurbs/opennurbs_photogrammetry.cpp \
		opennurbs/opennurbs_plane.cpp \
		opennurbs/opennurbs_planesurface.cpp \
		opennurbs/opennurbs_pluginlist.cpp \
		opennurbs/opennurbs_point.cpp \
		opennurbs/opennurbs_pointcloud.cpp \
		opennurbs/opennurbs_pointgeometry.cpp \
		opennurbs/opennurbs_pointgrid.cpp \
		opennurbs/opennurbs_polycurve.cpp \
		opennurbs/opennurbs_polyedgecurve.cpp \
		opennurbs/opennurbs_polyline.cpp \
		opennurbs/opennurbs_polylinecurve.cpp \
		opennurbs/opennurbs_quaternion.cpp \
		opennurbs/opennurbs_rand.cpp \
		opennurbs/opennurbs_revsurface.cpp \
		opennurbs/opennurbs_rtree.cpp \
		opennurbs/opennurbs_sort.cpp \
		opennurbs/opennurbs_sphere.cpp \
		opennurbs/opennurbs_string.cpp \
		opennurbs/opennurbs_sum.cpp \
		opennurbs/opennurbs_sumsurface.cpp \
		opennurbs/opennurbs_surface.cpp \
		opennurbs/opennurbs_surfaceproxy.cpp \
		opennurbs/opennurbs_textlog.cpp \
		opennurbs/opennurbs_torus.cpp \
		opennurbs/opennurbs_unicode.cpp \
		opennurbs/opennurbs_userdata.cpp \
		opennurbs/opennurbs_uuid.cpp \
		opennurbs/opennurbs_viewport.cpp \
		opennurbs/opennurbs_workspace.cpp \
		opennurbs/opennurbs_wstring.cpp \
		opennurbs/opennurbs_xform.cpp \
		opennurbs/opennurbs_zlib.cpp \
		opennurbs/opennurbs_zlib_memory.cpp


ON_OBJ= on_3dm_attributes.o \
		on_3dm_settings.o \
		on_annotation2.o \
		on_arc.o \
		on_arccurve.o \
		on_archive.o \
		on_array.o \
		on_beam.o \
		on_bezier.o \
		on_brep.o \
		on_circle.o \
		on_curve.o \
		on_defines.o \
		on_detail.o \
		on_dimstyle.o \
		on_geometry.o \
		on_hatch.o \
		on_instance.o \
		on_intersect.o \
		on_layer.o \
		on_light.o \
		on_line.o \
		on_linecurve.o \
		on_linetype.o \
		on_massprop.o \
		on_material.o \
		on_mesh.o \
		on_nurbscurve.o \
		on_nurbssurface.o \
		on_object.o \
		on_plane.o \
		on_planesurface.o \
		on_plus.o \
		on_point.o \
		on_pointcloud.o \
		on_pointgeometry.o \
		on_pointgrid.o \
		on_polycurve.o \
		on_polylinecurve.o \
		on_quaternion.o \
		on_rtree.o \
		on_sphere.o \
		on_surface.o \
		on_textlog.o \
		on_userdata.o \
		on_viewport.o \
		on_xform.o \
		stringholder.o \
		stdafx.o \
		opennurbs/opennurbs_3dm_attributes.o \
		opennurbs/opennurbs_3dm_properties.o \
		opennurbs/opennurbs_3dm_settings.o \
		opennurbs/opennurbs_annotation.o \
		opennurbs/opennurbs_annotation2.o \
		opennurbs/opennurbs_arc.o \
		opennurbs/opennurbs_arccurve.o \
		opennurbs/opennurbs_archive.o \
		opennurbs/opennurbs_array.o \
		opennurbs/opennurbs_base32.o \
		opennurbs/opennurbs_base64.o \
		opennurbs/opennurbs_beam.o \
		opennurbs/opennurbs_bezier.o \
		opennurbs/opennurbs_beziervolume.o \
		opennurbs/opennurbs_bitmap.o \
		opennurbs/opennurbs_bounding_box.o \
		opennurbs/opennurbs_box.o \
		opennurbs/opennurbs_brep.o \
		opennurbs/opennurbs_brep_extrude.o \
		opennurbs/opennurbs_brep_io.o \
		opennurbs/opennurbs_brep_isvalid.o \
		opennurbs/opennurbs_brep_region.o \
		opennurbs/opennurbs_brep_tools.o \
		opennurbs/opennurbs_brep_v2valid.o \
		opennurbs/opennurbs_circle.o \
		opennurbs/opennurbs_color.o \
		opennurbs/opennurbs_compress.o \
		opennurbs/opennurbs_cone.o \
		opennurbs/opennurbs_crc.o \
		opennurbs/opennurbs_curve.o \
		opennurbs/opennurbs_curveonsurface.o \
		opennurbs/opennurbs_curveproxy.o \
		opennurbs/opennurbs_cylinder.o \
		opennurbs/opennurbs_defines.o \
		opennurbs/opennurbs_detail.o \
		opennurbs/opennurbs_dimstyle.o \
		opennurbs/opennurbs_ellipse.o \
		opennurbs/opennurbs_embedded_file.o \
		opennurbs/opennurbs_error.o \
		opennurbs/opennurbs_error_message.o \
		opennurbs/opennurbs_evaluate_nurbs.o \
		opennurbs/opennurbs_extensions.o \
		opennurbs/opennurbs_font.o \
		opennurbs/opennurbs_fsp.o \
		opennurbs/opennurbs_geometry.o \
		opennurbs/opennurbs_group.o \
		opennurbs/opennurbs_hatch.o \
		opennurbs/opennurbs_instance.o \
		opennurbs/opennurbs_intersect.o \
		opennurbs/opennurbs_knot.o \
		opennurbs/opennurbs_layer.o \
		opennurbs/opennurbs_light.o \
		opennurbs/opennurbs_line.o \
		opennurbs/opennurbs_linecurve.o \
		opennurbs/opennurbs_linetype.o \
		opennurbs/opennurbs_lookup.o \
		opennurbs/opennurbs_material.o \
		opennurbs/opennurbs_math.o \
		opennurbs/opennurbs_matrix.o \
		opennurbs/opennurbs_memory.o \
		opennurbs/opennurbs_memory_util.o \
		opennurbs/opennurbs_mesh.o \
		opennurbs/opennurbs_mesh_ngon.o \
		opennurbs/opennurbs_mesh_tools.o \
		opennurbs/opennurbs_morph.o \
		opennurbs/opennurbs_nurbscurve.o \
		opennurbs/opennurbs_nurbssurface.o \
		opennurbs/opennurbs_nurbsvolume.o \
		opennurbs/opennurbs_object.o \
		opennurbs/opennurbs_object_history.o \
		opennurbs/opennurbs_objref.o \
		opennurbs/opennurbs_offsetsurface.o \
		opennurbs/opennurbs_optimize.o \
		opennurbs/opennurbs_photogrammetry.o \
		opennurbs/opennurbs_plane.o \
		opennurbs/opennurbs_planesurface.o \
		opennurbs/opennurbs_pluginlist.o \
		opennurbs/opennurbs_point.o \
		opennurbs/opennurbs_pointcloud.o \
		opennurbs/opennurbs_pointgeometry.o \
		opennurbs/opennurbs_pointgrid.o \
		opennurbs/opennurbs_polycurve.o \
		opennurbs/opennurbs_polyedgecurve.o \
		opennurbs/opennurbs_polyline.o \
		opennurbs/opennurbs_polylinecurve.o \
		opennurbs/opennurbs_quaternion.o \
		opennurbs/opennurbs_rand.o \
		opennurbs/opennurbs_revsurface.o \
		opennurbs/opennurbs_rtree.o \
		opennurbs/opennurbs_sort.o \
		opennurbs/opennurbs_sphere.o \
		opennurbs/opennurbs_string.o \
		opennurbs/opennurbs_sum.o \
		opennurbs/opennurbs_sumsurface.o \
		opennurbs/opennurbs_surface.o \
		opennurbs/opennurbs_surfaceproxy.o \
		opennurbs/opennurbs_textlog.o \
		opennurbs/opennurbs_torus.o \
		opennurbs/opennurbs_unicode.o \
		opennurbs/opennurbs_userdata.o \
		opennurbs/opennurbs_uuid.o \
		opennurbs/opennurbs_viewport.o \
		opennurbs/opennurbs_workspace.o \
		opennurbs/opennurbs_wstring.o \
		opennurbs/opennurbs_xform.o \
		opennurbs/opennurbs_zlib.o \
		opennurbs/opennurbs_zlib_memory.o


ZLIB_INC= opennurbs/zlib/crc32.h \
		opennurbs/zlib/deflate.h \
		opennurbs/zlib/inffast.h \
		opennurbs/zlib/inffixed.h \
		opennurbs/zlib/inflate.h \
		opennurbs/zlib/inftrees.h \
		opennurbs/zlib/trees.h \
		opennurbs/zlib/zconf.h \
		opennurbs/zlib/zlib.h \
		opennurbs/zlib/zutil.h \

ZLIB_SRC= opennurbs/zlib/adler32.c \
		opennurbs/zlib/compress.c \
		opennurbs/zlib/crc32.c \
		opennurbs/zlib/deflate.c \
		opennurbs/zlib/infback.c \
		opennurbs/zlib/inffast.c \
		opennurbs/zlib/inflate.c \
		opennurbs/zlib/inftrees.c \
		opennurbs/zlib/trees.c \
		opennurbs/zlib/uncompr.c \
		opennurbs/zlib/zutil.c


ZLIB_OBJ= opennurbs/zlib/adler32.o \
		opennurbs/zlib/compress.o \
		opennurbs/zlib/crc32.o \
		opennurbs/zlib/deflate.o \
		opennurbs/zlib/infback.o \
		opennurbs/zlib/inffast.o \
		opennurbs/zlib/inflate.o \
		opennurbs/zlib/inftrees.o \
		opennurbs/zlib/trees.o \
		opennurbs/zlib/uncompr.o \
		opennurbs/zlib/zutil.o


#EXAMPLE_OBJ = opennurbs/example_read/example_read.o \
#      opennurbs/example_write/example_write.o \
#      opennurbs/example_brep/example_brep.o \
#      opennurbs/example_userdata/example_ud.o \
#      opennurbs/example_userdata/example_userdata.o \
#      opennurbs/example_roundtrip/example_roundtrip.o
#
#EXAMPLES = opennurbs/example_read/example_read \
#      opennurbs/example_write/example_write \
#      opennurbs/example_brep/example_brep \
#      opennurbs/example_roundtrip/example_roundtrip \
#      opennurbs/example_userdata/example_userdata
#
#EXAMPLE_GL_OBJ = opennurbs/example_gl/example_gl.o \
#      opennurbs_gl.o
#
#EXAMPLE_GL = opennurbs/example_gl/example_gl

all : $(OPENNURBS_LIB_FILE) $(OPENNURBS_SHARED_LIB_FILE) #$(EXAMPLES)

#gl : $(OPENNURBS_LIB_FILE) $(EXAMPLE_GL)

opennurbs/opennurbs_gl.o : opennurbs/opennurbs_gl.h $(ON_INC)

#opennurbs/example_userdata/example_ud.o : opennurbs/example_userdata/example_ud.h $(ON_INC)

$(ON_OBJ) : $(ON_INC)

$(ZLIB_OBJ) : $(ZLIB_INC)

$(OPENNURBS_LIB_FILE) : $(ON_OBJ) $(ZLIB_OBJ)
	-$(RM) $@
	$(AR) $@ $(ON_OBJ) $(ZLIB_OBJ)
	$(RANLIB) $@

$(OPENNURBS_SHARED_LIB_FILE):  $(OPENNURBS_LIB_FILE)
	-$(MKDIR) build/Release-linux
	-$(CCCSHARED) -o $(OPENNURBS_SHARED_LIB_FILE) $(ON_OBJ) $(ZLIB_OBJ)
	-$(CP) $(OPENNURBS_SHARED_LIB_FILE) build/Release-linux/$(OPENNURBS_SHARED_LIB_FILE) 

#example_read/example_read : example_read/example_read.o example_userdata/example_ud.o $(OPENNURBS_LIB_FILE)
#	$(LINK) $(LINKFLAGS) example_read/example_read.o example_userdata/example_ud.o -L. -l$(OPENNURBS_LIB_NAME) -lm -o $@
#
#example_write/example_write : example_write/example_write.o example_userdata/example_ud.o $(OPENNURBS_LIB_FILE)
#	$(LINK) $(LINKFLAGS) example_write/example_write.o example_userdata/example_ud.o -L. -l$(OPENNURBS_LIB_NAME) -lm -o $@
#
#example_brep/example_brep : example_brep/example_brep.o $(OPENNURBS_LIB_FILE)
#	$(LINK) $(LINKFLAGS) example_brep/example_brep.o -L. -l$(OPENNURBS_LIB_NAME) -lm -o $@
#
#example_userdata/example_userdata : example_userdata/example_userdata.o $(OPENNURBS_LIB_FILE)
#	$(LINK) $(LINKFLAGS) example_userdata/example_userdata.o -L. -l$(OPENNURBS_LIB_NAME) -lm -o $@
#
#example_roundtrip/example_roundtrip : example_roundtrip/example_roundtrip.o $(OPENNURBS_LIB_FILE)
#	$(LINK) $(LINKFLAGS) example_roundtrip/example_roundtrip.o -L. -l$(OPENNURBS_LIB_NAME) -lm -o $@
#
#LINK_GL = -lglaux -lglu -lgl
#LINK_XWIN = -lXmu -lXi -lXext -lX11

#$(EXAMPLE_GL) : example_gl/example_gl.o ./opennurbs_gl.o $(OPENNURBS_LIB_FILE)
#	$(LINK) $(LINKFLAGS) example_gl/example_gl.o ./opennurbs_gl.o -L. -l$(OPENNURBS_LIB_NAME) $(LINK_GL) $(LINK_XWIN) -lm -o $@

clean :
	-$(RM) $(OPENNURBS_LIB_FILE)
	-$(RM) $(ON_OBJ)
	-$(RM) $(ZLIB_OBJ)
	-$(RM) $(OPENNURBS_SHARED_LIB_FILE)
	-$(RM) build/Release-linux/$(OPENNURBS_SHARED_LIB_FILE)
#	-$(RM) $(EXAMPLE_OBJ)
#	-$(RM) $(EXAMPLE_GL_OBJ)
#	-$(RM) $(EXAMPLES)
#	-$(RM) $(EXAMPLE_GL)
