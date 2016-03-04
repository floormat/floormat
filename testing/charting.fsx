#I "bin/Debug/"
#I "C:/Users/Administrator/dev/floormat/floormat/bin/Debug/"
#r "floormat.exe"
#r "System.Windows.Forms"
#r "System.Windows.Forms.DataVisualization"
#r "WindowsFormsIntegration"
#r "FSharp.Charting.dll"
open FSharp.Charting
open FSharp.Charting.ChartTypes

module FsiAutoShow = 
   fsi.AddPrinter(fun (ch:FSharp.Charting.ChartTypes.GenericChart) -> ch.ShowChart() |> ignore; "(Chart)")

//Chart.Line [ 1 .. 10 ]
