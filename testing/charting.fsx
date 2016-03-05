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
    let range = 700
    let sz = 2000
    let mid = sz / 2
    let r = xorshift.prng_state.new_prng()
    let hist = Array.zeroCreate sz : uint64 []
    let c = (float sz) / (float range)
    for i in 0 .. 1000000 - 1 do
        let x = r.next_normal(0.2)
        let idx = x * (float range) |> int |> (+) mid
        if idx >= 0 && idx < hist.Length then
            hist.[idx] <- hist.[idx] + 1UL
    let div = float(Array.sum hist)
#if !INTERACTIVE
    for i in 0 .. hist.Length - 1 do
        if hist.[i] > 0UL then
            let x = float(i - mid) / (float sz)
            printfn "%f %f" x (float(hist.[i]) / div * 100.)
#else
    let data = [
        for i in 0 .. sz - 1 ->
            let x = float(i - mid) * c / (float sz)
            x, float(hist.[i]) / div
    ]
    Chart.Point data
#endif
hist ()
