﻿Namespace EventHandling
    '''<summary>
    ''' <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    ''' class. DO NOT create instances of this class yourself. It is the
    ''' responsibility of Rhino to create an instance of this class.</para>
    ''' <para>To complete plug-in information, please also see all PlugInDescription
    ''' attributes in AssemblyInfo.vb (you might need to click "Project" ->
    ''' "Show All Files" to see it in the "Solution Explorer" window).</para>
    '''</summary>
    Public Class EventHandlingPlugIn
        Inherits Rhino.PlugIns.PlugIn

        Shared _instance As EventHandlingPlugIn

        Public Sub New()
            _instance = Me
        End Sub

        '''<summary>Gets the only instance of the EventHandlingPlugIn plug-in.</summary>
        Public Shared ReadOnly Property Instance() As EventHandlingPlugIn
            Get
                Return _instance
            End Get
        End Property


        ' You can override methods here to change the plug-in behavior on
        ' loading and shut down, add options pages to the Rhino _Option command
        ' and mantain plug-in wide options in a document.
    End Class
End Namespace