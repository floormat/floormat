module flush_denormals

module detail =
    open System
    open System.Runtime.InteropServices
    open FSharp.NativeInterop

    [<DllImport("msvcrt.dll", EntryPoint = "_controlfp_s", CallingConvention = CallingConvention.Cdecl)>]
    extern int fp_control(UIntPtr current, uint32 flags, uint32 mask);

    [<DllImport("msvcrt.dll", EntryPoint = "_statusfp", CallingConvention = CallingConvention.Cdecl)>]
    extern uint32 fp_status();

    [<Literal>]
    let MCW_DN = 0x03000000u
    [<Literal>]
    let DN_SAVE = 0x00000000u
    [<Literal>]
    let DN_FLUSH = 0x01000000u
    [<Literal>]
    let MCW_EM = 0x0008001Fu
    [<Literal>]
    let FP_MASK = MCW_DN ||| MCW_EM

    let default_flags = (fp_status() &&& FP_MASK) ||| MCW_EM

    let enable_fast_math () =
        let ret = fp_control(UIntPtr.Zero, MCW_EM ||| DN_FLUSH, FP_MASK)
        if ret <> 0 then failwith "failed to set fast math"
        ()
    let disable_fast_math () =
        let ret = fp_control(UIntPtr.Zero, default_flags, FP_MASK)
        if ret <> 0 then failwith "failed to unset fast math"
        ()
    let is_unix = let p = Environment.OSVersion.Platform |> int in p = 4 || p = 128

let inline enable_fast_math () = if not detail.is_unix then detail.enable_fast_math ()
let inline disable_fast_math () = if not detail.is_unix then detail.disable_fast_math ()
let inline set_fast_math flag =
    if not detail.is_unix then
        if flag then enable_fast_math() else disable_fast_math()
