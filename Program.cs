using Azure.Storage.Blobs;
using DotNetEnv;

// Load environment variables from the .env file
Env.Load();

Console.WriteLine("Azure Blob Storage exercise\n");

// Run the examples asynchronously, wait for the results before proceeding
ProcessAsync().GetAwaiter().GetResult();

Console.WriteLine("Press enter to exit the sample application.");
Console.ReadLine();

static async Task ProcessAsync()
{
    // Copy the connection string from the portal in the variable below.
    const string connectionStringKey = "AZ204_BLOB_CONN_STRING";

    var storageConnectionString = Environment.GetEnvironmentVariable(connectionStringKey)
    ?? throw new ArgumentNullException($"{connectionStringKey} environment variable cannot be null");

    // Create a client that can authenticate with a connection string
    var blobServiceClient = new BlobServiceClient(storageConnectionString);

    // Create a unique name for the container
    var containerName = "wtblob" + Guid.NewGuid().ToString();

    // Create the container and return a container client object
    var containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
    Console.WriteLine("A container named '" + containerName + "' has been created. " +
    "\nTake a minute and verify in the portal." +
    "\nNext a file will be created and uploaded to the container.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();

    // Create a local file in the ./data/ directory for uploading and downloading
    var localPath = "./data/";
    var fileName = "wtfile" + Guid.NewGuid().ToString() + ".txt";
    var localFilePath = Path.Combine(localPath, fileName);

    // Write text to the file
    await File.WriteAllTextAsync(localFilePath, "Hello, World!");

    // Get a reference to the blob
    var blobClient = containerClient.Value.GetBlobClient(fileName);

    Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

    // Open the file and upload its data
    using var uploadFileStream = File.OpenRead(localFilePath);
    await blobClient.UploadAsync(uploadFileStream);
    uploadFileStream.Close();


    Console.WriteLine("\nThe file was uploaded. We'll verify by listing" +
            " the blobs next.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();

    // List blobs in the container
    Console.WriteLine("Listing blobs...");
    await foreach (var blobItem in containerClient.Value.GetBlobsAsync())
    {
        Console.WriteLine("\t" + blobItem.Name);
    }

    Console.WriteLine("\nYou can also verify by looking inside the " +
            "container in the portal." +
            "\nNext the blob will be downloaded with an altered file name.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();

    // Download the blob to a local file
    // Append the string "DOWNLOADED" before the .txt extension
    var downloadFilePath = localFilePath.Replace(".txt", "DOWNLOAD.txt");

    Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);

    var download = await blobClient.DownloadToAsync(downloadFilePath);

    Console.WriteLine("\nLocate the local file in the data directory created earlier to verify it was downloaded.");
    Console.WriteLine("The next step is to delete the container and local files.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();

    // Delete the container and clean up local files created
    Console.WriteLine("\n\nDeleting blob container...");
    await containerClient.Value.DeleteAsync();

    Console.WriteLine("Deleting the local source and downloaded files...");
    File.Delete(localFilePath);
    File.Delete(downloadFilePath);

    Console.WriteLine("Finished cleaning up.");
}