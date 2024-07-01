using Azure.Storage.Queues;
using JumpStartCS.Orleans.Grains.Filters;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;

await Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
    siloBuilder.UseAzureStorageClustering(configureOptions: options =>
    {
        options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
    });

    siloBuilder.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "JumpstartCSCluster";
        options.ServiceId = "JumpstartCSService";
    });

    siloBuilder.AddAzureTableGrainStorage("tableStorage", configureOptions: options =>
    {
        options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
    });

    siloBuilder.AddAzureBlobGrainStorage("blobStorage", configureOptions: options =>
    {
        options.BlobServiceClient = new Azure.Storage.Blobs.BlobServiceClient("UseDevelopmentStorage=true;");
    });

    siloBuilder.UseAzureTableReminderService(configureOptions: options =>
    {
        options.Configure(o => o.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;"));
    });

    siloBuilder.AddAzureTableTransactionalStateStorageAsDefault(configureOptions: options =>
    {
        options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
    });

    siloBuilder.UseTransactions();

    siloBuilder.AddAzureQueueStreams("StreamProvider", optionsBuilder =>
    {
        optionsBuilder.Configure(options => { options.QueueServiceClient = new QueueServiceClient("UseDevelopmentStorage=true;"); });
    })
    .AddAzureTableGrainStorage("PubSubStore", configureOptions: options =>
    {
        options.Configure(o => o.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;"));
    });

    siloBuilder.AddIncomingGrainCallFilter<LoggingIncomingGrainCallFilter>();

        //siloBuilder.Configure<GrainCollectionOptions>(options =>
        //{
        //    options.CollectionQuantum = TimeSpan.FromSeconds(20);

        //    options.CollectionAge = TimeSpan.FromSeconds(20);
        //});

    }).RunConsoleAsync();
