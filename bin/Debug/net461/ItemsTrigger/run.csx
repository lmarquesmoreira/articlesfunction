#r "Microsoft.Azure.Documents.Client"
using Microsoft.Azure.Documents;
using System.Collections.Generic;
using System;
public static async Task Run(IReadOnlyList<Document> input, TraceWriter log)
{
    log.Verbose("Document count " + input.Count);
	log.Verbose("First document Id " + input[0].Id);
}