using Azure.Data.Tables;
using recorder_fn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recorder_fn.Services
{
	public class CallRecordingService : ICallRecordingService
	{
		private readonly TableClient _tableClient;

		public CallRecordingService(string connectionString, string tableName)
		{
			var serviceClient = new TableServiceClient(connectionString);
			serviceClient.CreateTableIfNotExists(tableName);
			_tableClient = serviceClient.GetTableClient(tableName);
		}

		public async Task SaveCallRecordingAsync(CallRecordingEntity entity)
		{
			await _tableClient.UpsertEntityAsync(entity);
		}

		public async Task<CallRecordingEntity> GetCallRecordingAsync(string rowId, string partitionKey)
		{
			var entity = await _tableClient.GetEntityAsync<CallRecordingEntity>(partitionKey, rowId);

			return entity.Value;
		}

		public async Task UpdateCallRecordingAsync(CallRecordingEntity entity)
		{
			await _tableClient.UpsertEntityAsync(entity);
		}


	}
}
