#I "bin/Debug"
#r "floormat.exe"
#r "System.Windows.Forms"
#r "System.Windows.Forms.DataVisualization"
#r "WindowsFormsIntegration"
#r "FSharp.Charting.dll"
open FSharp.Charting
open FSharp.Charting.ChartTypes
open xorshift

module FsiAutoShow = 
   fsi.AddPrinter(fun (ch:FSharp.Charting.ChartTypes.GenericChart) -> ch.ShowChart() |> ignore; "(Chart)")

//Chart.Line [ 1 .. 10 ]

let hist () =
    let sz = 1000
    let mid = sz / 2
    let space = 1./4.
    let iters = 10000000
    let r = xorshift.prng_state.new_prng()
    let hist = Array.zeroCreate sz
    for i in 0 .. iters - 1 do
        let x = r.next_normal(1.)
        let idx = round(x * (float mid) * space) |> int |> (+) mid
        if idx >= 0 && idx < hist.Length then
            hist.[idx] <- hist.[idx] + 1
    let sum = Array.sum hist |> float
    let data = [
        for i in 0 .. sz - 1 ->
            let x = float(i - mid)
            x, float(hist.[i]) / sum
    ]
    Chart.Line data
hist ()
