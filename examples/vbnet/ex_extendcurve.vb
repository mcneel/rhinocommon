﻿Imports System.Linq
Imports Rhino
Imports Rhino.Geometry
Imports Rhino.DocObjects
Imports Rhino.Commands
Imports Rhino.Input
Imports Rhino.Input.Custom

Namespace examples_vb
  Public Class ExtendCurveCommand
    Inherits Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbExtendCurve"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As RunMode) As Result
      Dim boundary_obj_refs As ObjRef() = Nothing
      Dim rc = RhinoGet.GetMultipleObjects("Select boundary objects", False, ObjectType.AnyObject, boundary_obj_refs)
      If rc <> Result.Success Then
        Return rc
      End If
      If boundary_obj_refs Is Nothing OrElse boundary_obj_refs.Length = 0 Then
        Return Result.[Nothing]
      End If

      Dim gc = New GetObject()
      gc.SetCommandPrompt("Select curve to extend")
      gc.GeometryFilter = ObjectType.Curve
      gc.GeometryAttributeFilter = GeometryAttributeFilter.OpenCurve
      gc.[Get]()
      If gc.CommandResult() <> Result.Success Then
        Return gc.CommandResult()
      End If
      Dim curve_obj_ref = gc.[Object](0)

      Dim curve = curve_obj_ref.Curve()
      If curve Is Nothing Then
        Return Result.Failure
      End If
      Dim t As Double
      If Not curve.ClosestPoint(curve_obj_ref.SelectionPoint(), t) Then
        Return Result.Failure
      End If
      Dim curve_end = If(t <= curve.Domain.Mid, CurveEnd.Start, CurveEnd.[End])

      Dim geometry = boundary_obj_refs.[Select](Function(obj) obj.Geometry())
      Dim extended_curve = curve.Extend(curve_end, CurveExtensionStyle.Line, geometry)
      If extended_curve IsNot Nothing AndAlso extended_curve.IsValid Then
        If Not doc.Objects.Replace(curve_obj_ref.ObjectId, extended_curve) Then
          Return Result.Failure
        End If
        doc.Views.Redraw()
      Else
        RhinoApp.WriteLine("No boundary object was intersected so curve not extended")
        Return Result.[Nothing]
      End If

      Return Result.Success
    End Function
  End Class
End Namespace