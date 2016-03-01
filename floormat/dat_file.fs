module dat_file

type System.IO.Stream with
    member inline public this.read_byte() = byte (this.ReadByte() &&& 0xff)
    
    member public this.read_le_int32() = 
        let b1 = this.read_byte()
        let b2 = this.read_byte()
        let b3 = this.read_byte()
        let b4 = this.read_byte()
        (int (b4) <<< 24) ||| (int (b3) <<< 16) ||| (int (b2) <<< 8) ||| int (b1)
    
    member inline public this.read_le_uint32() = uint32 (this.read_le_int32())

module detail = 
    open System.IO
    open Ionic.Zlib
    open System.IO.MemoryMappedFiles

    type gzip_stream = ZlibStream
    
    type gzip_mode = CompressionMode
    
    type hash<'k, 'v> = System.Collections.Generic.Dictionary<'k, 'v>
    
    type DatEntry = 
        { filename : string
          is_compressed : bool
          old_size : int
          new_size : int
          offset : int
        }
    
    [<Literal>]
    let FILE_GZIP = 0x00000002u
    
    [<Literal>]
    let FILE_DIRECTORY = 0x00000400u
    
    [<Literal>]
    let DAT_FOOTER_SIZE = 28
    
    [<Literal>]
    let DAT_MAGIC = 1145132081
    
    type DatFile(dat_filename) = 
        let mutable files_ = null
        let mutable handle_ = null

        do  let info = FileInfo(dat_filename)
            // CAVEAT specifying default 0, 0 for offset and length doesn't map the entire file
            handle_ <- MemoryMappedFile.CreateFromFile(dat_filename, FileMode.Open, null, info.Length, MemoryMappedFileAccess.Read)
            use stream' = handle_.CreateViewStream(0L, int64 info.Length, MemoryMappedFileAccess.Read)
            use stream = new BufferedStream(stream', 4096)
            files_ <- DatFile.read_files(stream)

        interface System.IDisposable with
            member this.Dispose() = handle_.Dispose()

        member this.files with get() = files_
        
        static member private read_files(stream : Stream) = 
            stream.Seek(int64 (stream.Length - int64(DAT_FOOTER_SIZE)), SeekOrigin.Begin) |> ignore
            for _ in 1..16 do
                stream.read_byte() |> ignore
            let magic = stream.read_le_int32()
            if magic <> DAT_MAGIC then raise (InvalidDataException())
            let pool_size = stream.read_le_int32()
            let dict_size = stream.read_le_int32()
            stream.Seek(int64 (-dict_size), SeekOrigin.End) |> ignore
            let entry_count = stream.read_le_int32()
            let files = hash(entry_count)
            for i in 0..(entry_count - 1) do
                let filename_len = stream.read_le_int32() - 1
                let filename = Array.zeroCreate filename_len
                for j in 0..filename_len - 1 do
                    filename.[j] <- stream.read_byte() |> char
                stream.read_byte() |> ignore
                let unused_ptr = stream.read_le_uint32()
                let flags = stream.read_le_int32() |> uint32
                let old_size = stream.read_le_int32()
                let new_size = stream.read_le_int32()
                let offset = stream.read_le_int32()
                let is_compressed = flags &&& FILE_GZIP <> 0u
                let is_directory = flags &&& FILE_DIRECTORY <> 0u
                if not is_directory then 
                    let filename = System.String(DatFile.sanitize_filename(filename) : char [])
                    files.Add(filename, 
                              { filename = filename
                                is_compressed = is_compressed
                                old_size = old_size
                                new_size = new_size
                                offset = offset })
            files
        
        static member private sanitize_filename(chars) = 
            let inline fast_tolower chr = 
                if chr >= 'A' && chr <= 'Z' then chr + ' '
                else chr
            for i in 0..chars.Length - 1 do
                let mutable c = chars.[i]
                c <- fast_tolower c
                if c = '\\' then c <- '/'
                chars.[i] <- c
            chars
        
        member this.Item 
            with get filename = 
                let entry = files_.[filename]
                let ret = handle_.CreateViewStream(int64 entry.offset, int64 entry.new_size, MemoryMappedFileAccess.Read)
                // CAVEAT Stream.ReadByte() allocates a 1-byte array each time so use BufferedStream
                if entry.is_compressed then 
                    new BufferedStream(new gzip_stream(ret, CompressionMode.Decompress, false), 4096) :> Stream
                else
                    new BufferedStream(ret, 4096) :> Stream
        
        member this.filename = dat_filename

type DatFile = detail.DatFile
type DatDict = (string, detail.DatEntry) detail.hash
type DatEntry = detail.DatEntry
type DatPair = (string, DatEntry) System.Collections.Generic.KeyValuePair