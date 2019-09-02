﻿Imports System.Windows.Forms

Imports TwainDotNet
Imports TwainDotNet.TwainNative
Imports TwainDotNet.WinFroms

Public Class MainForm
	ReadOnly areaSettings As New AreaSettings(0.1F, 5.7F, 0.1F + 2.6F, 5.7F + 2.6F)

#Disable Warning IDE0069 ' Disposable fields should be disposed
	''' <summary>
	''' Twain scanning library
	''' </summary>
	ReadOnly twain As Twain
#Enable Warning IDE0069 ' Disposable fields should be disposed

	''' <summary>
	''' The current scan settings.
	''' </summary>
	Dim settings As ScanSettings

    ''' <summary>
    ''' The current list of images (only the latest displayed in the Form).
    ''' </summary>
    Dim images As List(Of System.Drawing.Bitmap)

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Twain needs a hook into this Form's message loop to work:
        twain = New Twain(New WinFormsWindowMessageHook(Me))

        ' Add a handler to grab each image as it comes off the scanner
        AddHandler twain.TransferImage,
            Sub(sender As Object, args As TwainDotNet.TransferImageEventArgs)
                If (args.HBitmap <> IntPtr.Zero) Then
                    Dim renderer As New TwainDotNet.Win32.BitmapRenderer(args.HBitmap)
                    Using renderer
                        pictureBox1.Image = renderer.RenderToBitmap()
                    End Using


                    images.Add(pictureBox1.Image)

                    widthLabel.Text = String.Format("Width: {0}", pictureBox1.Image.Width)
                    heightLabel.Text = String.Format("Height: {0}", pictureBox1.Image.Height)
                End If
            End Sub

        ' Re-enable the form after scanning completes
        AddHandler twain.ScanningComplete,
            Sub(sender As Object, e As TwainDotNet.ScanningCompleteEventArgs)
                Enabled = True
			End Sub

		AddHandler FormClosed, AddressOf MainForm_FormClosed
	End Sub

	Private Sub MainForm_FormClosed(ByVal sender As System.Object, ByVal e As FormClosedEventArgs)
		RemoveHandler FormClosed, AddressOf MainForm_FormClosed

		If (Not (twain Is Nothing)) Then
			twain.Dispose()
		End If
	End Sub

	Private Sub SelectSource_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles selectSource.Click
		' Show the "select scanning source" dialog
		twain.SelectSource()
	End Sub

	Private Sub Scan_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles scan.Click
		' Disable the Form until scanning completes
		Enabled = False

		' Clear off any images from the last run
		images = New List(Of System.Drawing.Bitmap)

		' Grab the current settings
		settings = New ScanSettings With {
			.UseDocumentFeeder = useAdfCheckBox.Checked,
			.ShowTwainUI = useUICheckBox.Checked,
			.ShowProgressIndicatorUI = showProgressIndicatorUICheckBox.Checked,
			.UseDuplex = useDuplexCheckBox.Checked
		}
		If (blackAndWhiteCheckBox.Checked) Then
			settings.ColourSetting = ColourSetting.BlackAndWhite
			settings.Dpi = 200
		Else
			settings.ColourSetting = ColourSetting.Colour
			settings.Dpi = 300
		End If
		If (checkBoxArea.Checked) Then
			settings.Units = Units.Centimeters
			settings.Area = areaSettings
		End If
		settings.ShouldTransferAllPages = True

		settings.AutomaticRotate = autoRotateCheckBox.Checked
		settings.AutomaticBorderDetection = autoDetectBorderCheckBox.Checked

		Try
			' Start scanning. Depending on the settings above dialogs from the scanner driver may be displayed.
			twain.StartScanning(settings)

		Catch ex As Exception
			MessageBox.Show(ex.Message)
			Enabled = True
		End Try
	End Sub

	Private Sub SaveButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles saveButton.Click

		If (Not (pictureBox1.Image Is Nothing)) Then
			Using sfd As New SaveFileDialog()

				' TODO: save each image in "images" as a page in a TIFF file

				If sfd.ShowDialog() = DialogResult.OK Then
					pictureBox1.Image.Save(sfd.FileName)
				End If
			End Using
		End If
	End Sub

	Private Sub DiagnosticsButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles diagnosticsButton.Click
		' Dump out diagnostics from the current source
		Dim diagnostics As New Diagnostics(New WinFormsWindowMessageHook(Me))
	End Sub
End Class
