namespace AIAgent.Data.Entities;

public class AIPolicyEntity
{
	public int Id { get; set; }
	public string PolicyCode { get; set; }
	public string SectionCode { get; set; }
	public string DocumentName { get; set; }
	public string Content { get; set; }
	public int ChunckId { get; set; }
	public Vector Embedding { get; set; }
}
