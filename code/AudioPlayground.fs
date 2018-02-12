module AudioPlayground

open NAudio.Wave
open System
open NAudio.CoreAudioApi
open System.Threading

type NoiseOutput () =
    let waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1) // samplerate:44.1kHz, mono
    let bytesPerSample = waveFormat.BitsPerSample / 8
    let random = Random()

    interface IWaveProvider with
        member __.WaveFormat with get() = waveFormat

        member __.Read (buffer, offset, count) =
            let mutable writeIndex = 0
            let putSample sample buffer =
                // convert float to byte array
                let bytes = BitConverter.GetBytes((float32)sample)
                // blit into correct position in buffer
                Array.blit bytes 0 buffer (offset + writeIndex) bytes.Length
                // update position
                writeIndex <- writeIndex + bytes.Length

            let nSamples = count / bytesPerSample
            let rescale value = (value * 2.0) - 1.0
            for _ in [0 .. nSamples - 1] do
                let sample = random.NextDouble() |> rescale
                putSample sample buffer
            // return the number of bytes written
            nSamples * bytesPerSample

[<EntryPoint>]
let main _ =
    let output = new WasapiOut(AudioClientShareMode.Shared, 1)
    NoiseOutput () |> output.Init
    output.Play ()
    Thread.Sleep 2000
    output.Stop ()
    0