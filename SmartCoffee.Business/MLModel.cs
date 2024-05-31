using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SmartCoffee.Data.Entities;
using Document = Lucene.Net.Documents.Document;
using Microsoft.ML.OnnxRuntimeGenAI;
using Tokenizer = Microsoft.ML.OnnxRuntimeGenAI.Tokenizer;

namespace SmartCoffee.Business
{
    public class MLModel
    {
        string indexPath = Path.Combine(AppContext.BaseDirectory, "objectDB");
        private readonly Analyzer _analyzer;
        private readonly FSDirectory _directory;
        string modelPath = /*"/storage/emulated/0/onnx/cpu_and_mobile/cpu-int4-rtn-block-32";*/ "C:\\onnx\\cpu_and_mobile\\cpu-int4-rtn-block-32-acc-level-4";
        string systemPrompt = @"You are a knowledgeable and friendly assistant that Guilherme Oliveira, a student and Fontys from Open Learning.
            Answer the following question as clearly and concisely as possible, providing any relevant information about coffee related equipment, like espresso machines, grinders, accessories, and everything coffee and espresso related that the user might ask.";

        public MLModel()
        {
            _analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            _directory = FSDirectory.Open(indexPath);

        }

        public async Task IndexFiles(Product product)
        {
            try
            {

                using var writer = new IndexWriter(_directory, _analyzer, IndexWriter.MaxFieldLength.UNLIMITED);

                string finalText = "";

                string html = await GetHtmlAsync(product.URL);

                // Parse the HTML content
            
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);

                // Extract all text nodes
                document.DocumentNode.Descendants()
                                 .Where(n => n.Name == "script" || n.Name == "style")
                                 .ToList()
                                 .ForEach(n => n.Remove());

                // Extract and print the text
                string textNodes = ExtractText(document.DocumentNode);

                // Print all relevant text
                if (textNodes != null)
                {
                    finalText += textNodes;
                }

                var doc = new Document();
                doc.Add(new Field("name", product.Name, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
                doc.Add(new Field("information", finalText, Field.Store.YES, Field.Index.ANALYZED));

                writer.AddDocument(doc);
                writer.Commit();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public string Retrieve(string queryText)
        {
            var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "information", _analyzer);
            var query = parser.Parse(queryText);
            var searcher = new IndexSearcher(_directory);
            var hits = searcher.Search(query, 5).ScoreDocs;

            foreach (var hit in hits)
            {
                var foundDoc = searcher.Doc(hit.Doc);
                return foundDoc.Get("information");
            }

            return string.Empty;
        }

        public string Inference(string query)
        {
            try
            {
                var model = new Model(modelPath);

                var tokenizer = new Tokenizer(model);

                var info = Retrieve(query);

                var finalQuery = $"Based on this information: '{info}', answer this question: {query}";
                var fullPrompt = $"<|system|>{systemPrompt}<|end|><|user|>{finalQuery}<|end|><|assistant|>";

                var tokens = tokenizer.Encode(fullPrompt);

                var generatorParams = new GeneratorParams(model);
                generatorParams.SetSearchOption("max_length", 2048);
                generatorParams.SetSearchOption("past_present_share_buffer", false);
                generatorParams.SetInputSequences(tokens);

                var generator = new Generator(model, generatorParams);

                string finalGeneration = "";

                while (!generator.IsDone())
                {
                    generator.ComputeLogits();
                    generator.GenerateNextToken();
                    var outputTokens = generator.GetSequence(0);
                    var newToken = outputTokens.Slice(outputTokens.Length - 1, 1);
                    var output = tokenizer.Decode(newToken);
                    finalGeneration += output;
                }

                return finalGeneration;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        async Task<string> GetHtmlAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        string ExtractText(HtmlNode node)
        {
            if (!node.HasChildNodes)
            {
                return node.InnerText;
            }

            return string.Join(" ", node.ChildNodes.Select(child => ExtractText(child)).Where(text => !string.IsNullOrWhiteSpace(text)));
        }
    }
}
