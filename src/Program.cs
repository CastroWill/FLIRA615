using Flir.Atlas.Image;
using Flir.Atlas.Live.Device;
using Flir.Atlas.Live.Discovery;
using System;
using System.IO;

internal class Program
{
    // Classe responsável por gerenciar a conexão e a lógica da câmera térmica
    public class CameraController
    {
        public Camera Camera { get; private set; }

        // Método para conectar à câmera térmica usando um endereço IP dinâmico
        public void Connect(string ipAddress, string savePath)
        {
            try
            {
                Disconnect(); // Certifica-se de liberar qualquer recurso anterior antes de conectar
                var device = CameraDeviceInfo.Create(ipAddress, Interface.Gigabit, ImageFormat.FlirFileFormat);

                if (device != null)
                {
                    Camera = new ThermalCamera();

                    // Subscrição ao evento para capturar imagens
                    Camera.ImageInitialized += (sender, e) => OnImageInitialized(sender, e, savePath);

                    Camera.Connect(device);
                    Console.WriteLine("Conectado à câmera!");
                }
                else
                {
                    Console.WriteLine("Não foi possível conectar à câmera.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar: {ex.Message}");
            }
        }

        // Método para desconectar a câmera e liberar recursos
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

        // Evento acionado quando a imagem é inicializada
        private void OnImageInitialized(object sender, EventArgs e, string savePath)
        {
            try
            {
                var image = GetImage();
                if (image == null)
                {
                    Console.WriteLine("Imagem não está disponível no momento.");
                    return;
                }

                image.EnterLock();
                if (image is ThermalImage thermalImage)
                {
                    thermalImage.Scale.IsAutoAdjustEnabled = true;
                    Console.WriteLine("Imagem inicializada e ajustada. Salvando...");

                    // Salvar a imagem no caminho especificado
                    thermalImage.SaveSnapshot(savePath);
                    Console.WriteLine($"Imagem salva em: {savePath}");
                }
                image.ExitLock();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar imagem: {ex.Message}");
            }
        }

        // Método auxiliar para obter a imagem atual da câmera
        private ImageBase GetImage()
        {
            return Camera?.GetImage();
        }
    }

    public static void Main(string[] args)
    {
        // Leitura de parâmetros de linha de comando ou uso de valores padrão
        string ipAddress = args.Length > 0 ? args[0] : "169.254.45.1";
        string savePath = args.Length > 1 ? args[1] : @"c:\tmp\test.jpg";

        Console.WriteLine("Iniciando controle da câmera térmica...");

        var controller = new CameraController();
        controller.Connect(ipAddress, savePath);

        Console.WriteLine("Pressione qualquer tecla para desconectar e sair...");
        Console.ReadKey();

        controller.Disconnect();
    }
}
