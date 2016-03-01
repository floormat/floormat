module game

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

type game() as this =
    inherit Game()
    let mutable sb = null
    let graphics = new GraphicsDeviceManager(this)

    interface System.IDisposable with
        member this.Dispose() =
            if sb <> null then sb.Dispose()

            graphics.Dispose()
            base.Dispose()

    override this.Initialize() =
        base.Initialize()

    override this.LoadContent() =
        this.Content.RootDirectory <- "Content"
        sb <- new SpriteBatch(this.GraphicsDevice)

    override this.UnloadContent() =
        ()

    override this.Update(tm : GameTime) =
        base.Update(tm)

    override this.Draw(tm : GameTime) =
        this.GraphicsDevice.Clear(Color.CornflowerBlue);
        base.Draw(tm)

[<EntryPoint>]
[<System.STAThread>]
let main argv =
    use g = new game()
    g.Run() |> ignore
    0