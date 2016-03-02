module converter

open dat_file
open art_file

type bm = System.Drawing.Bitmap
type c = System.Drawing.Color
type path = System.IO.Path
type fmt = System.Drawing.Imaging.PixelFormat
type rect = System.Drawing.Rectangle
type lock_mode = System.Drawing.Imaging.ImageLockMode
type marshal = System.Runtime.InteropServices.Marshal
type intptr = System.IntPtr
type bitmap_data = System.Drawing.Imaging.BitmapData
type dir = System.IO.Directory

//[<EntryPoint>]
let convert argv = 
    let num = 4
    use dat_file = new DatFile(@"C:\Users\Administrator\dev\floormat\arcanum-data\arcanum" + num.ToString() + ".dat")
    let dir = @"e:\arcanum-img" + num.ToString()

    let process_image (kv : DatPair) =
        let filename = kv.Value.filename
        if filename.EndsWith(".art") then
            let dir = dir + "/" + filename
            let art = ArtFile.from_file_entry(dat_file, filename)
            let bm_data = bitmap_data()
            for rotation in 0 .. art.rotation_count - 1 do
                for palette in 0 .. art.palette_count - 1 do
                    for frame_idx in 0 .. art.frame_count - 1 do
                        let frame = art.rotations.[rotation, frame_idx]

                        use bm = new bm(frame.width, frame.height)
                        bm.LockBits(rect(0, 0, frame.width, frame.height), lock_mode.WriteOnly, bm.PixelFormat, bm_data) |> ignore
                        for y in 0 .. frame.height - 1 do
                            let ptr = bm_data.Scan0 + nativeint(y * bm_data.Stride)
                            let idx = y * frame.width
                            marshal.Copy(frame.data.[palette], idx, ptr, frame.width)
                        bm.UnlockBits(bm_data)

                        let dir = dir + "-r" + rotation.ToString() + "-p" + palette.ToString()
                        System.IO.Directory.CreateDirectory(dir) |> ignore
                        let filename = dir + "/" + frame_idx.ToString() +  ".bmp"
                        ()
                        //bm.Save(filename)

    for kv in dat_file.files do process_image kv


    (*
    System.IO.Directory.CreateDirectory(dir + "/art/wall/") |> ignore
    use stream = System.IO.File.OpenWrite(dir + "/art/wall/structure.mes")
    use stream2 = dat_file.["art/wall/structure.mes"]
    stream2.CopyTo(stream)
    *)

    printfn ""
    0
    