# ChunkExecutor
Processes long list in smaller chunks to avoid timeouts and blocking resources for too long time.

## Usage
<pre>
var executor = new SerialChunkExecutor(10);
var processedItemCount = await executor.Execute(myDatabaseExportFunction,myListIds);
</pre>