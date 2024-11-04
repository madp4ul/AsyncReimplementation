using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsyncReimplementation;
public static class Fetch
{
    public static Promise<string> FetchAsync(string url)
    {
        return new Promise<string>((resolve, reject) =>
        {
            Uri uri = new Uri(url);
            var host = uri.Host;
            var port = uri.Port == -1 ? 80 : uri.Port;

            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(host);
                var ip = addresses[0];

                Socket socket = CreateSocket(ip, port, reject);
                StartRequest(socket, uri, host, resolve, reject);
            }
            catch (Exception ex)
            {
                reject(ex);
            }
        });
    }

    private static Socket CreateSocket(IPAddress ip, int port, Action<Exception> reject)
    {
        try
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(new IPEndPoint(ip, port), ar =>
            {
                try
                {
                    socket.EndConnect(ar);
                }
                catch (Exception ex)
                {
                    reject(ex);
                    socket.Close();
                }
            }, null);
            return socket;
        }
        catch (Exception ex)
        {
            reject(ex);
            return null;
        }
    }

    private static void StartRequest(Socket socket, Uri uri, string host, Action<string> resolve, Action<Exception> reject)
    {
        var request = BuildHttpRequest(uri, host);
        byte[] requestBytes = Encoding.ASCII.GetBytes(request);
        socket.Send(requestBytes);

        var responseBuilder = new StringBuilder();
        var buffer = new byte[1024];

        ReceiveResponse(socket, buffer, responseBuilder, resolve, reject);
    }

    private static string BuildHttpRequest(Uri uri, string host)
    {
        return $"GET {uri.PathAndQuery} HTTP/1.1\r\nHost: {host}\r\nConnection: close\r\n\r\n";
    }

    private static void ReceiveResponse(Socket socket, byte[] buffer, StringBuilder responseBuilder, Action<string> resolve, Action<Exception> reject)
    {
        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);

        void ReceiveCallback(IAsyncResult iar)
        {
            try
            {
                int bytesRead = socket.EndReceive(iar);
                if (bytesRead > 0)
                {
                    responseBuilder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
                }
                else
                {
                    resolve(responseBuilder.ToString());
                    socket.Close();
                }
            }
            catch (Exception ex)
            {
                reject(ex);
                socket.Close();
            }
        }
    }
}