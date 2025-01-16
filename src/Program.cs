using Flir.Atlas.Image;
using Flir.Atlas.Live.Device;
using Flir.Atlas.Live.Discovery;
using System;

internal class Program
{
    private ImageBase GetImage()
    {
        return Camera?.GetImage();
    }

    public Camera Camera { get; set; }

    public static void Main(string[] args)
    {
        Console.WriteLine("Realizando conexão com a câmera");

        var program = new Program();
        program.ConnectToA615();

        Console.WriteLine("Pressione qualquer tecla para desconectar e sair...");
        Console.ReadKey();

        program.Disconnect();
    }

    public void Disconnect()
    {
        if (Camera != null)
        {
            Camera.StopGrabbing();
            Camera.Disconnect();
            Camera.Dispose();
            Camera = null;
            Console.WriteLine("Câmera desconectada!");
        }
    }

    public void ConnectToA615()
    {
        Disconnect();

        var device = CameraDeviceInfo.Create("169.254.20.1", Interface.Gigabit, ImageFormat.FlirFileFormat);

        if (device != null)
        {
            Camera = new ThermalCamera();

            // Subscrição ao evento de imagem inicializada
            Camera.ImageInitialized += ThermalCamera_ImageInitialized;

            Camera.Connect(device);

            Console.WriteLine("Conectado à câmera!");
        }
        else
        {
            Console.WriteLine("Não foi possível executar a operação.");
        }
    }

    private void ThermalCamera_ImageInitialized(object sender, EventArgs e)
    {
        var image = GetImage();
        if (image == null)
        {
            Console.WriteLine("Imagem não está disponível no momento.");
            return;
        }

        image.EnterLock();
        try
        {
            if (image is ThermalImage thermalImage)
            {
                thermalImage.Scale.IsAutoAdjustEnabled = true;
                Console.WriteLine("Imagem inicializada e ajustada. Salvando...");

                // Salvar a imagem após ajustes
                thermalImage.SaveSnapshot(@"c:\tmp\test.jpg");
                Console.WriteLine("Imagem salva em c:\\tmp\\test.jpg");
            }
        }
        finally
        {
            image.ExitLock();
        }
    }
}
