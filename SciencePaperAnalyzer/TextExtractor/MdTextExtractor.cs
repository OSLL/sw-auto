using AnalyzeResults.Presentation;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SentenceHtml = AnalyzeResults.Presentation.Sentence;
using WordHtml = AnalyzeResults.Presentation.Word;

namespace TextExtractor
{
    public class MdTextExtractor : ITextExtractor
    {
        public string ExtractTextFromFileStream(Stream fileStream)
        {
            fileStream.Position = 0;
            using (var reader = new StreamReader(fileStream, Encoding.UTF8))
            {
                var result = reader.ReadToEnd();
                return result;
            }
        }

        public List<Section> ExtractStructureFromFileStream(Stream fileStream, bool extractTitle = false)
        {
            List<Section> sections = new List<Section>();
            string mdContent = ExtractTextFromFileStream(fileStream);

            // create builder and build the pipeline for parsing md file
            var builder = new Markdig.MarkdownPipelineBuilder();
            Markdig.MarkdownExtensions.UseAdvancedExtensions(builder);

            var pipeline = builder.Build();

            var doc = Markdig.Markdown.Parse(mdContent, pipeline);

            bool capturedTitle = false; // is the title captured?
            bool isNextSectReferenceList = false;
            // for each element in the document
            foreach (var ele in doc)
            {
                var start = ele.Span.Start;
                var length = ele.Span.Length;

                //Console.WriteLine(ele.ToString());
                if (ele is HeadingBlock) // header
                {
                    var blockText = mdContent.Substring(start, length).Trim();
                    var level = blockText.LastIndexOf(((HeadingBlock)ele).HeaderChar);
                    blockText = blockText.Replace(((HeadingBlock)ele).HeaderChar, ' ').Trim();
                    var section = new Section();

                    // assume paper's title is the first heading block
                    if (!capturedTitle && extractTitle)
                    {
                        section.Type = SectionType.PaperTitle;
                        capturedTitle = true;
                    }
                    else if (level < 2)
                    {
                        section.Type = SectionType.SectionTitle;
                    }
                    else
                    {
                        section.Type = SectionType.Text;
                    }
                    //Console.WriteLine(blockText);
                    // if header name matched -> get next section as references list
                    if (blockText == "Список литературы")
                    {
                        //Console.WriteLine("---> nextSect");
                        isNextSectReferenceList = true;
                    }
                    section.OriginalText = blockText;
                    sections.Add(section);
                }
                else
                if (ele is ParagraphBlock) 
                {
                    var blockText = mdContent.Substring(start, length).Trim();
                    var section = new Section();
                    // if isNextSectReferenceList is set (last header found is Literature list) then this section is reference list, else normal text
                    //Console.WriteLine(blockText);
                    if (isNextSectReferenceList)
                    {
                        //Console.WriteLine("---> reflist");
                        section.Type = SectionType.ReferencesList;
                        isNextSectReferenceList = false;
                    }
                    else
                    {
                        section.Type = SectionType.Text;
                        // for normal text section (~paragraph), remove line breaks
                        blockText = blockText.Replace('\n', ' ');
                    }
                    section.OriginalText = blockText;
                    sections.Add(section);
                }
                else
                if (ele is ListBlock) // List block contains Block and Inlines. Block can be ParagraphBlock or ListItemBlock or anything else
                {
                    // if isNextSectReferenceList is set (last header found is Literature list) then this section is reference list, else normal text
                    if (isNextSectReferenceList) // in case this is reference list, keep it as a single block, as it will be process later on
                    {
                        var section = new Section();
                        var blockText = mdContent.Substring(start, length).Trim();
                        //Console.WriteLine("---> reflist");
                        section.Type = SectionType.ReferencesList;
                        isNextSectReferenceList = false;
                        section.OriginalText = blockText;
                        sections.Add(section);
                    }
                    else // if it's not a reference list, keep each list entry as a separate paragraph
                    {                        
                        foreach (var block in ((ListBlock)ele).Descendants<ParagraphBlock>())
                        {
                            var section = new Section();
                            var blockText = mdContent.Substring(block.Span.Start, block.Span.Length);
                            // for normal text section (~paragraph), remove line breaks
                            blockText = blockText.Replace('\n', ' ');

                            section.Type = SectionType.Text;
                            section.OriginalText = blockText;
                            sections.Add(section);
                        }
                    }
                }
            }
            return sections;
        }
    }
}
