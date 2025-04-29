using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;

namespace recorder_fn.Services
{
	public class QueueService
	{
		private readonly QueueClient _queueClient;

		public QueueService(string connectionString, string queueName)
		{
			_queueClient = new QueueClient(connectionString, queueName,  new QueueClientOptions
			{
				MessageEncoding = QueueMessageEncoding.Base64
			});
		}

		public async Task SendMessageAsync(string message)
		{
			await _queueClient.SendMessageAsync(message);
		}
		
		
	}
}
