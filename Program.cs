using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = null;

        bool exit = false;
            try
            {
                while (true && !exit)
                {
                    int port;
                    bool isPortRight = false;

                    while (!isPortRight)
                    {
                        Console.Write("Введите порт для подключения к серверу либо exit для выхода: ");
                        string portInput = Console.ReadLine();

                        if (int.TryParse(portInput, out port))
                        {
                            client = new TcpClient();

                            try
                            {
                                client.Connect("localhost", port);
                                isPortRight = true;
                                Console.WriteLine($"Подключение к серверу на порту {port} выполнено успешно.");
                            }
                            catch (SocketException)
                            {
                                isPortRight = false;
                                Console.WriteLine($"Не удалось подключиться к серверу на порту {port}. Пожалуйста, попробуйте другой порт.");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Ошибка: {e.Message}");
                                isPortRight = false;
                            }
                        }
                        else
                        {
                            if (portInput.Equals("exit"))
                            {
                                exit = true;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Некорректный порт. Пожалуйста, введите целое число.");
                            }
                        }
                    }

                    string savePath="";
                    while (true && !exit)
                    {
                        Console.Write("Введите путь для сохранения файлов либо exit для выхода: ");
                        savePath = Console.ReadLine();

                        if (savePath.Equals("exit"))
                        {
                            exit = true;
                            break;
                        }

                        if (!Directory.Exists(savePath))
                        {
                            Console.WriteLine("Указанный путь не существует.");
                            continue;
                        }

                        break;
                    }
                    if (!exit) {
                        NetworkStream stream = client.GetStream();
                        StreamReader reader = new StreamReader(stream);
                        StreamWriter writer = new StreamWriter(stream);

                        writer.WriteLine("get_files");
                        writer.Flush();

                        List<string> fileList = new List<string>();

                        string file;
                        while ((file = reader.ReadLine()) != "END_OF_LIST")
                        {
                            Console.WriteLine($"{fileList.Count + 1}. {file}");
                            fileList.Add(file);
                        }

                        int choiceNumber;
                        string choice = null;
                        while (true)
                        {
                            Console.Write("Выберите номер файла для скачивания (или введите 'exit' для выхода): ");
                            string userInput = Console.ReadLine();

                            if (userInput.ToLower() == "exit")
                            {
                                choice = userInput.ToLower();
                                break;
                            }

                            if (int.TryParse(userInput, out choiceNumber) && choiceNumber >= 1 && choiceNumber <= fileList.Count)
                            {
                                choice = fileList[choiceNumber - 1];
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Некорректные данные. Пожалуйста, введите номер файла из списка или 'exit'.");
                            }
                        }

                        if (choice != null && choice != "exit")
                        {
                            writer.WriteLine(choice);
                            writer.Flush();

                            string response = reader.ReadLine();
                            if (response == "Directory not accessible")
                            {
                                Console.WriteLine("Выбранная директория не доступна. Выберите другую.");
                                continue;
                            }

                            string filePath = Path.Combine(savePath, choice);
                            using (FileStream fileStream = File.Create(filePath))
                            {
                                byte[] buffer = new byte[1024];
                                int bytesRead;
                                bytesRead = stream.Read(buffer, 0, buffer.Length);
                                if (bytesRead == 0)
                                {
                                    Console.WriteLine("Ошибка: файл не был получен.");
                                    continue;
                                }
                                else
                                {
                                    fileStream.Write(buffer, 0, bytesRead);
                                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        fileStream.Write(buffer, 0, bytesRead);
                                    }
                                }
                            }

                            Console.WriteLine($"Файл успешно скачан и сохранен по пути: {filePath}");

                            Console.WriteLine("Открываем файл в браузере...");
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
                        }
                        else
                        {
                            Console.WriteLine("Скачивание отменено.");
                        }

                        // Закрытие только потока, не клиента, чтобы соединение оставалось открытым для последующих запросов
                        reader.Close();
                        writer.Close();
                        stream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                client?.Close();
            }
        }
    }
}
