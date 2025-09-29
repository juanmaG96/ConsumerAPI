using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Text;

using Newtonsoft.Json;
using ConsumerAPI.Models;
using ConsumerAPI.Services;
using Microsoft.Extensions.Options;
using ConsumerAPI.Config;
using Microsoft.AspNetCore.Connections;

namespace ConsumerAPI.Services
{
    public class RabbitMQService<TClass>: IRabbitMQService<TClass> where TClass : ClassMessageRMQ 
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private readonly RabbitMQSettings _config;
        private readonly string _queueName;   // Cola por defecto
        private List<TClass> listClass = new List<TClass>();
        //private string? QueueName { get; set; }

        public RabbitMQService(IOptions<RabbitMQSettings> config)
        {
            _config = config.Value;
            _queueName = "qDefault";

            _connectionFactory = new ConnectionFactory()
            {
                //Uri = new Uri(_config.RabbitMQUrl),
                UserName = _config.UserName,
                Password = _config.Password,
                HostName = _config.HostName,
                Port = int.Parse(_config.Port)
            };

            string rabbitUrl = _config.RabbitMQUrl;
            _connection = _connectionFactory.CreateConnection(rabbitUrl);
            _channel = _connection.CreateModel();
        }

        /*
         * recibir un mensaje de Rabbit sin confirmacion de recepcion
         * Retorna una clase MessageMQ que contienen el nombre de la queue y el mensaje como string
         * 
        */
        public async Task<List<TClass>> ReceiveMessage(string? queueName = null)
        {
            this.listClass.Clear();
            List<TClass> aux = new List<TClass>();

            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string jsonMessage = Encoding.UTF8.GetString(body);
                TClass? message = JsonConvert.DeserializeObject<TClass>(jsonMessage);
                if (message != null)
                {
                    aux.Add(message);
                    this.listClass.Add(message);
                }
            };

            // string? _queueName = queueName != "" ? queueName : this.QueueName;
            // Si no se pasa una cola, usa la definida en appsettings.json
            string queueToUse = string.IsNullOrEmpty(queueName) ? _queueName : queueName;
            _channel.BasicConsume(queue: queueToUse,
                                 autoAck: true,
                                 consumer: consumer);

            Func<int, Task> myAnonymousTaskFunction = async (x) =>
            {
                await Task.Delay(1000);     // simulate an async operation
            };

            await myAnonymousTaskFunction(10);
            return aux;
        }

        /*
        * recibir un mensaje de Rabbit con confirmacion de recepcion
        * Retorna una clase MessageMQ que contienen el nombre de la queue y el mensaje como string
        * 
        */
        public async Task<List<TClass>> ReceiveMessageWithAcknowledgment(string? queueName = null)
        {
            //List<TClass> aux = new List<TClass>();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string jsonMessage = Encoding.UTF8.GetString(body);
                TClass? message = JsonConvert.DeserializeObject<TClass>(jsonMessage);
                if (message != null)
                    this.listClass.Add(message);
                _channel.BasicAck(ea.DeliveryTag, false);   // envia el acuse de recibo (ack) al servidor RabbitMQ
            };

            string queueToUse = string.IsNullOrEmpty(queueName) ? _queueName : queueName;
            _channel.BasicConsume(queue: queueToUse,
                                 autoAck: false,   // autoAck en false desactiva el modo de confirmacion automatica
                                 consumer: consumer);
            Func<int, Task> myAnonymousTaskFuntion = async (x) =>
            {
                await Task.Delay(1000);     // simulate an async operation
            };

            await myAnonymousTaskFuntion(10);
            return this.listClass;
        }

        public void CloseConnection()
        {
            _channel.Close();
            _connection.Close();
        }
        public int GetMessageCount(string? queueName = null) {
            string queueToUse = string.IsNullOrEmpty(queueName) ? _queueName : queueName;
            QueueDeclareOk cola = _channel.QueueDeclarePassive(queueToUse);
            return (int)cola.MessageCount;
        }
    }
}