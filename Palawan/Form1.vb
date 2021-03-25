Imports System.Data.SqlClient

Public Class Form1

    Private Enum nLocatorSystem
        NeuesSystem
        GeschossSystem
        ErdgeschossSystem
        PalettenPlatzOderUnbekannt
        ErdgeschossSystem10
        Palettenplatz_X
        Palettenplatz_Y
        VHalle
        Loft
        CrossDocking
    End Enum

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim oData As SqlConnection = Nothing
        Dim sSKU = String.Empty
        Dim nCartons As Integer = 0
        Dim i As Integer

        Dim oFile As System.IO.StreamWriter
        oFile = My.Computer.FileSystem.OpenTextFileWriter("C:\Users\m.graf\Desktop\whsplit.txt", False)

        Using oFile

            Using cn = New SqlConnection(My.Settings.SQL_Cotton)
                cn.Open()

                Dim cmd As New SqlCommand(My.Resources.SQL1000, cn)
                cmd.Parameters.Add("@DatumVON", SqlDbType.DateTime).Value = "01.11.2019"
                cmd.Parameters.Add("@DatumBIS", SqlDbType.DateTime).Value = "31.10.2020 23:59:59"

                Dim reader = cmd.ExecuteReader

                If reader.HasRows Then
                    Do While reader.Read
                        If sSKU <> reader!Artikelnummer Then
                            sSKU = reader!Artikelnummer
                            For i = 1 To reader!AnzGanzKartons
                                oFile.WriteLine(sSKU)
                            Next

                        End If
                    Loop
                End If
                reader.Close()
                reader = Nothing

                cn.Close()
            End Using
            oFile.Close()
        End Using
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        Dim oData As SqlConnection = Nothing
        Dim sLocator = String.Empty
        Dim nLocCount As Integer
        Dim nLocSystem As nLocatorSystem
        Dim nPosition As Integer

        Dim sLocatorEdited = String.Empty
        Dim i As Integer

        Dim oTable As New DataTable
        With oTable.Columns
            .Add("Mandant", GetType(Short))
            .Add("LegacyLocator", GetType(String))
            .Add("Halle", GetType(String))
            .Add("Reihe", GetType(String))
            .Add("Ebene", GetType(Short))
            .Add("Position", GetType(Short))
            .Add("Breite", GetType(Decimal))
            .Add("Bodenhoehe", GetType(Decimal))

            .Add("Fachhoehe", GetType(Decimal))
            .Add("Logik", GetType(String))
            .Add("ReiheModulo", GetType(Short))
            .Add("Tiefe", GetType(Decimal))
        End With

        oTable.PrimaryKey = New DataColumn() {oTable.Columns("LegacyLocator")}


        'DataTable mit Reiheninformationen laden
        Using cn = New SqlConnection(My.Settings.SQL_CCLEAP)
            Dim oAdapter = New SqlDataAdapter(My.Resources.SQL1002, cn)

            oAdapter.Fill(oTable)
        End Using



        Dim oFile As System.IO.StreamWriter
        oFile = My.Computer.FileSystem.OpenTextFileWriter("C:\Users\m.graf\Desktop\LocatorListe.csv", False)

        Dim oFileError As System.IO.StreamWriter
        oFileError = My.Computer.FileSystem.OpenTextFileWriter("C:\Users\m.graf\Desktop\LocatorListeErrors.csv", False)

        Using oFileError
            oFileError.WriteLine("Artikelnummer;Bezeichnung1;Farbe;Groesse;Locator;Locator Office-Line;Locator-System")

            Using oFile

                oFile.WriteLine("Artikelnummer;Bezeichnung1;Farbe;Groesse;Locator;Locator Office-Line;Locator-System;Modulo;Halle;Reihe;Position;Ebene")

                Using cn = New SqlConnection(My.Settings.SQL_Cotton)
                    cn.Open()

                    Dim cmd As New SqlCommand(My.Resources.SQL1001, cn)
                    Dim reader = cmd.ExecuteReader
                    Dim nModulu As Short
                    Dim sNeu_Halle As String = "Unbekannt"
                    Dim sNeu_Reihe As String = "Unbekannt"
                    Dim nNeu_Position As Integer = -1
                    Dim nNeu_Ebene As Integer = -1

                    If reader.HasRows Then
                        Do While reader.Read


                            If sLocator <> reader!USER_DTKommLager Then
                                sLocator = reader!USER_DTKommLager

                                nLocCount = reader!LOC_SKUCount
                                nLocSystem = nDetermineLocatorSystem(sLocator)
                                nPosition = 0
                                i = 0

                                Dim oRow = oTable.Rows.Find(reader!USER_DTKommLager)

                                If Not oRow Is Nothing Then

                                    nModulu = oRow.Field(Of Short)("ReiheModulo")
                                    sNeu_Halle = oRow.Field(Of String)("Halle")
                                    sNeu_Reihe = oRow.Field(Of String)("Reihe")
                                    nNeu_Position = oRow.Field(Of Short)("Position")
                                    nNeu_Ebene = oRow.Field(Of Short)("Ebene")

                                Else
                                    nModulu = 1
                                    sNeu_Halle = "Unbekannt"
                                    sNeu_Reihe = "Unbekannt"
                                    nNeu_Position = -1
                                    nNeu_Ebene = -1
                                End If

                                sLocatorEdited = String.Empty
                            End If

                            Select Case nLocSystem
                                Case nLocatorSystem.GeschossSystem
                                    nPosition = reader!USER_DTKommLager.ToString.Substring(4, 2) + (i * nModulu)
                                    sLocatorEdited = reader!USER_DTKommLager.ToString.Remove(4, 2)
                                    sLocatorEdited = sLocatorEdited.Insert(4, Format(nPosition, "00"))
                                    nWriteLine(oFile, reader, sLocatorEdited, sLocator, sParseLocatorSystem(nLocSystem), nModulu, sNeu_Halle, sNeu_Reihe, nNeu_Position, nNeu_Ebene)

                                Case nLocatorSystem.ErdgeschossSystem
                                    nPosition = reader!USER_DTKommLager.ToString.Substring(3, 2) + (i * nModulu)
                                    sLocatorEdited = reader!USER_DTKommLager.ToString.Remove(3, 2)
                                    sLocatorEdited = sLocatorEdited.Insert(3, Format(nPosition, "00"))
                                    nWriteLine(oFile, reader, sLocatorEdited, sLocator, sParseLocatorSystem(nLocSystem), nModulu, sNeu_Halle, sNeu_Reihe, nNeu_Position, nNeu_Ebene)

                                Case nLocatorSystem.NeuesSystem
                                    Dim bMatch As Boolean
                                    Dim nCounter As Integer

                                    Do While bMatch = False

                                        nCounter += 1
                                        nPosition = reader!USER_DTKommLager.ToString.Substring(5, 2) + (i * nModulu)
                                        sLocatorEdited = reader!USER_DTKommLager.ToString.Remove(5, 2)
                                        sLocatorEdited = sLocatorEdited.Insert(5, Format(nPosition, "00"))
                                        i += 1

                                        Dim oRow = oTable.Rows.Find(sLocatorEdited)

                                        If nCounter > 10 And oRow Is Nothing Then
                                            Debug.WriteLine("Kein Locator gefunden: " & reader!Artikelnummer)
                                            nWriteLineError(oFileError, reader, sLocatorEdited, sLocator, sParseLocatorSystem(nLocSystem) & "-Nicht Gefunden")
                                            nCounter = 0
                                        End If

                                        If Not oRow Is Nothing Then
                                            bMatch = True
                                            nWriteLine(oFile, reader, sLocatorEdited, sLocator, sParseLocatorSystem(nLocSystem), nModulu, sNeu_Halle, sNeu_Reihe, nNeu_Position, nNeu_Ebene)
                                            nCounter = 0
                                            bMatch = True
                                        End If
                                    Loop

                                Case nLocatorSystem.ErdgeschossSystem10
                                    nPosition = reader!USER_DTKommLager.ToString.Substring(6, 2) + (i * nModulu)
                                    sLocatorEdited = reader!USER_DTKommLager.ToString.Remove(6, 2)
                                    sLocatorEdited = sLocatorEdited.Insert(6, Format(nPosition, "00"))
                                    nWriteLine(oFile, reader, sLocatorEdited, sLocator, sParseLocatorSystem(nLocSystem), nModulu, sNeu_Halle, sNeu_Reihe, nNeu_Position, nNeu_Ebene)

                                Case nLocatorSystem.Palettenplatz_X
                                    nPosition = reader!USER_DTKommLager.ToString.Substring(3, 2) + (i * nModulu)
                                    sLocatorEdited = reader!USER_DTKommLager.ToString.Remove(3, 2)
                                    sLocatorEdited = sLocatorEdited.Insert(3, Format(nPosition, "00"))
                                    nWriteLine(oFile, reader, sLocatorEdited, sLocator, sParseLocatorSystem(nLocSystem), nModulu, sNeu_Halle, sNeu_Reihe, nNeu_Position, nNeu_Ebene)

                                Case nLocatorSystem.Palettenplatz_Y
                                    nPosition = reader!USER_DTKommLager.ToString.Substring(3, 2) + (i * nModulu)
                                    sLocatorEdited = reader!USER_DTKommLager.ToString.Remove(3, 2)
                                    sLocatorEdited = sLocatorEdited.Insert(3, Format(nPosition, "00"))
                                    nWriteLine(oFile, reader, sLocatorEdited, sLocator, sParseLocatorSystem(nLocSystem), nModulu, sNeu_Halle, sNeu_Reihe, nNeu_Position, nNeu_Ebene)



                                Case Else : nWriteLineError(oFileError, reader, sLocatorEdited, sLocator, sParseLocatorSystem(nLocSystem))

                            End Select

                            i += 1
                        Loop
                    End If
                    reader.Close()
                    reader = Nothing

                    cn.Close()
                End Using
                oFile.Close()
            End Using
        End Using

        MsgBox("Finish!")
    End Sub

    Private Sub nWriteLine(oFile As IO.StreamWriter,
                           oReader As SqlDataReader,
                           sLocator As String,
                           sLocatorOL As String,
                           sLocatorSystem As String,
                           nModulo As Integer,
                           sHalle As String,
                           sReihe As String,
                           nPosition As Integer,
                           nEbene As Integer)

        With oReader
            oFile.WriteLine(String.Concat(!Artikelnummer, ";", !Bezeichnung1, ";", !Farbe, ";", !Groesse, ";", sLocator, ";", sLocatorOL, ";", sLocatorSystem, ";", nModulo, ";", sHalle, ";", sReihe, ";", nPosition, ";", nEbene))
        End With

    End Sub

    Private Sub nWriteLineError(oFile As IO.StreamWriter,
                           oReader As SqlDataReader,
                           sLocator As String,
                           sLocatorOL As String,
                           sLocatorSystem As String)

        With oReader
            oFile.WriteLine(String.Concat(!Artikelnummer, ";", !Bezeichnung1, ";", !Farbe, ";", !Groesse, ";", sLocator, ";", sLocatorOL, ";", sLocatorSystem))
        End With

    End Sub

    Public Shared Function gbNewLocatorSystem(ByVal value As String) As Boolean
        If value.Length = 9 Then
            If value.Substring(1, 1) = "." AndAlso
                value.Substring(4, 1) = "_" AndAlso
                value.Substring(7, 1) = "." Then Return True
        End If

        Return False
    End Function

    Private Function sDetermineLocatorSystem(ByVal Value As String) As String
        Return sParseLocatorSystem(nDetermineLocatorSystem(Value))

    End Function

    Private Function nDetermineLocatorSystem(ByVal Value As String) As nLocatorSystem

        'Cross-Docking
        If Value.Length >= 3 Then
            If Strings.Left(Value, 3) = "1AA" Or Strings.Left(Value, 3) = "6AA" Then
                Return nLocatorSystem.CrossDocking
            End If
        End If

            'Neues System
            If Value.Length = 9 Then
            If Value.Substring(1, 1) = "." AndAlso
                Value.Substring(4, 1) = "_" AndAlso
                Value.Substring(7, 1) = "." Then Return nLocatorSystem.NeuesSystem
        End If

        'Loft
        If Value.Length >= 2 Then
            If Strings.Left(Value, 2) = "G4" Then
                Return nLocatorSystem.Loft
            End If
        End If

        'Geschosse
        If Value.Length = 8 Then
            If Strings.Left(Value, 1) = "G" And IsNumeric(Mid(Value, 2, 1)) And IsNumeric(Mid(Value, 5, 2)) Then
                Return nLocatorSystem.GeschossSystem
            End If
        End If

        'Palettenplatz X
        If Value.Length = 8 Then
            If Strings.Left(Value, 2) = "G3" And IsNumeric(Mid(Value, 2, 1)) And IsNumeric(Mid(Value, 4, 2)) Then
                Return nLocatorSystem.Palettenplatz_X
            End If
        End If

        'Palettenplatz Y
        If Value.Length = 8 Then
            If Strings.Left(Value, 2) = "G2" And IsNumeric(Mid(Value, 2, 1)) And IsNumeric(Mid(Value, 4, 2)) Then
                Return nLocatorSystem.Palettenplatz_Y
            End If
        End If

        'VHalle
        If Value.Length >= 2 Then
            If Strings.Left(Value, 1) = "+" Or Strings.Left(Value, 1) = "0" Then
                Return nLocatorSystem.VHalle
            End If
        End If

        'Erdgeschoss
        If Value.Length = 7 Then
            If Value.Substring(2, 1) = "." And IsNumeric(Value.Substring(3)) And IsNumeric(Value.Substring(4)) Then
                Return nLocatorSystem.ErdgeschossSystem
            End If
        End If

        'Erdgeschoss LEN10
        If Value.Length = 10 Then
            If Strings.Left(Value, 1) = "G" And IsNumeric(Mid(Value, 2, 1)) And IsNumeric(Mid(Value, 4, 2)) Then
                Return nLocatorSystem.ErdgeschossSystem10
            End If
        End If


        Return nLocatorSystem.PalettenPlatzOderUnbekannt
    End Function

    Private Function sParseLocatorSystem(nValue As nLocatorSystem) As String
        Select Case nValue
            Case nLocatorSystem.NeuesSystem : Return "Neues System"
            Case nLocatorSystem.GeschossSystem : Return "Geschoss System"
            Case nLocatorSystem.ErdgeschossSystem : Return "Erdgeschoss System"
            Case nLocatorSystem.ErdgeschossSystem10 : Return "Geschoss System (LEN10)"
            Case nLocatorSystem.VHalle : Return "Halle V"
            Case nLocatorSystem.PalettenPlatzOderUnbekannt : Return "Palettenplatz oder Unbekannt"
            Case nLocatorSystem.Palettenplatz_X : Return "Palettenplatz Halle X"
            Case nLocatorSystem.Palettenplatz_Y : Return "Palettenplatz Halle Y"
            Case nLocatorSystem.Loft : Return "Loft"
            Case nLocatorSystem.CrossDocking : Return "Cross-Docking Lagerplatz"

            Case Else : Return "#Fehler"
        End Select

    End Function

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim oData As SqlConnection = Nothing
        Dim sLocator = String.Empty
        Dim i2

        Dim sLocatorEdited = String.Empty
        Dim i As Integer

        Dim oFile As System.IO.StreamWriter
        oFile = My.Computer.FileSystem.OpenTextFileWriter("C:\Users\m.graf\Desktop\LocatorListeExplore.csv", False)

        Using oFile

            oFile.WriteLine("Halle;Reihe;PositionLinks;PositionRechts;AnzahlEbenen;Locator")

            Using cn = New SqlConnection(My.Settings.SQL_CCTausch)
                cn.Open()

                Dim cmd As New SqlCommand("SELECT * FROM _tmpLocatorSpiel", cn)
                Dim reader = cmd.ExecuteReader

                If reader.HasRows Then
                    Do While reader.Read
                        For i = reader!PositionLinks To reader!PositionRechts
                            i2 = 0

                            For i2 = 0 To reader!AnzahlEbenen - 1
                                oFile.WriteLine(String.Concat(reader!Halle, ";", reader!Reihe, ";", reader!PositionLinks, ";", reader!PositionRechts, ";", reader!AnzahlEbenen, ";",
                                                              String.Concat(reader!Reihe, ".", Format(i, "00"), ".", i2)))
                            Next
                        Next

                    Loop
                End If
                reader.Close()
                reader = Nothing

                cn.Close()
            End Using
            oFile.Close()
        End Using

        MsgBox("Finish!")

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim oCLS As New SKU2Loc

        oCLS.WriteData()

    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim oCLS As New FreieLocatoren
        oCLS.WriteData(True)

    End Sub
End Class
