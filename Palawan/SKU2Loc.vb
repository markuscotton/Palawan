Imports System.Data.SqlClient

Public Class SKU2Loc


    Public Sub WriteData()

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
            Dim oAdapter = New SqlDataAdapter(My.Resources.SQL1001, cn)

            oAdapter.Fill(oTable)
        End Using

        'Output File
        Dim oFile As System.IO.StreamWriter
        oFile = My.Computer.FileSystem.OpenTextFileWriter("C:\Users\m.graf\Desktop\LocatorListe.csv", False)
        oFile.WriteLine("Artikelnummer;Bezeichnung1;Farbe;Groesse;Locator;Locator Office-Line;Locator-System;Modulo;Halle;Reihe;Position;Ebene")

        Dim oFileError As System.IO.StreamWriter
        oFileError = My.Computer.FileSystem.OpenTextFileWriter("C:\Users\m.graf\Desktop\LocatorListeErrors.csv", False)
        oFileError.WriteLine("Artikelnummer;Bezeichnung1;Farbe;Groesse;Locator;Locator Office-Line;Locator-System")

        Dim sLocatorMerk = String.Empty
        Dim nLocCount As Integer
        Dim nPosition As Integer
        Dim i As Integer
        Dim nModulu As Short
        Dim sNeu_Halle As String = "Unbekannt"
        Dim sNeu_Reihe As String = "Unbekannt"
        Dim nNeu_Position As Integer = -1
        Dim nNeu_Ebene As Integer = -1
        Dim sLocatorEdited = String.Empty

        Using cn = New SqlConnection(My.Settings.SQL_Cotton)
            cn.Open()
            Dim cmd As New SqlCommand(My.Resources.SQL1003, cn)
            Dim reader = cmd.ExecuteReader

            If reader.HasRows Then
                Do While reader.Read

                    If sLocatorMerk <> reader!USER_DTKommLager Then
                        sLocatorMerk = reader!USER_DTKommLager

                        nLocCount = reader!LOC_SKUCount
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

                            nWriteLineError(oFileError, reader, reader!USER_DTKommLAger, "Locator nicht im System hinterlegt")
                            Continue Do
                        End If
                    End If

                    sLocatorEdited = String.Empty

                    Dim bFinish = False
                    Dim nCounter As Integer = 0
                    Do While bFinish = False
                        'Neuen Locator bauen
                        nPosition = reader!USER_DTKommLager.ToString.Substring(5, 2) + (i * nModulu)
                        sLocatorEdited = reader!USER_DTKommLager.ToString.Remove(5, 2)
                        sLocatorEdited = sLocatorEdited.Insert(5, Format(nPosition, "00"))

                        'Schauen ob der Locator Existiert
                        Dim oRow2 = oTable.Rows.Find(sLocatorEdited)

                        If oRow2 Is Nothing Then
                            'nix gefunden
                            nCounter += 1
                            'Für den nächsten Schleifendurchlauf i erhöhen
                            'Nach dem 10ten Versuch aufgeben
                            If nCounter >= 10 Then
                                nWriteLineError(oFileError, reader, reader!USER_DTKommLAger, "Kein Fortlaufender Locator gefunden")
                                bFinish = True
                            End If
                        Else
                            nWriteLine(oFile, reader, sLocatorEdited, sLocatorMerk, "NeuesSystem", nModulu, sNeu_Halle, sNeu_Reihe, nNeu_Position, nNeu_Ebene)
                            bFinish = True
                        End If
                        i += 1

                    Loop

                Loop
            End If
        End Using

        oFile.Close()
        oFileError.Close()

    End Sub


    Private Sub nWriteLineError(oFile As IO.StreamWriter,
                       oReader As SqlDataReader,
                       sLocatorOL As String,
                       sErrorMsg As String)

        With oReader
            oFile.WriteLine(String.Concat(!Artikelnummer, ";", !Bezeichnung1, ";", !Farbe, ";", !Groesse, ";", sLocatorOL, ";", sErrorMsg))
        End With

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
End Class
