Imports System.Data.SqlClient

Public Class FreieLocatoren

    Public Sub WriteData(bNurGanzeReihen As Boolean)

        Dim oTable As New DataTable
        With oTable.Columns
            .Add("Artikelnummer", GetType(String))
            .Add("LocatorEffektiv", GetType(String))
        End With

        oTable.PrimaryKey = New DataColumn() {oTable.Columns("LocatorEffektiv")}

        Using cn = New SqlConnection(My.Settings.SQL_Cotton)
            Dim oAdapter = New SqlDataAdapter(My.Resources.SQL1004, cn)

            oAdapter.Fill(oTable)
        End Using

        'Output-File
        Dim oFile As System.IO.StreamWriter
        oFile = My.Computer.FileSystem.OpenTextFileWriter("C:\Users\m.graf\Desktop\FreieLocatoren.csv", False)
        oFile.WriteLine("Halle;Reihe;Ebene;Locator;Frei;Fachhöhe")

        Dim sHalleMerk = String.Empty
        Dim sReiheMerk = String.Empty

        Dim sLocatorMerk = String.Empty
        Dim nLocCount As Integer


        Using cn = New SqlConnection(My.Settings.SQL_CCLEAP)
            cn.Open()
            Dim cmd As New SqlCommand(My.Resources.SQL1005, cn)
            Dim reader = cmd.ExecuteReader

            If reader.HasRows Then
                Do While reader.Read

                    If reader!Halle <> sHalleMerk Then
                        sLocatorMerk = String.Empty
                        nLocCount = 0

                        sHalleMerk = reader!Halle
                    End If

                    If bNurGanzeReihen Then
                        If reader!Reihe <> sReiheMerk Then
                            sLocatorMerk = String.Empty
                            nLocCount = 0

                            sReiheMerk = reader!Reihe
                        End If
                    End If

                    Dim oArtikel = oTable.Rows.Find(reader!Locator)


                    If oArtikel Is Nothing Then
                        If sLocatorMerk = String.Empty Then
                            sLocatorMerk = reader!Locator
                            nLocCount += 1
                        Else
                            nLocCount += 1
                        End If
                    Else
                        If nLocCount >= 1 Then _
                            oFile.WriteLine(String.Concat(reader!Halle, ";", reader!Reihe, ";", reader!Ebene, ";", sLocatorMerk, ";", nLocCount, ";", reader!Fachhoehe))

                        sLocatorMerk = String.Empty
                        nLocCount = 0
                    End If
                Loop
                reader.Close()
            End If
        End Using

        oFile.Close()
        MsgBox("Finish!")
    End Sub

End Class
