namespace AIAgent.Data.Entities;

public class PolicyEntity
{
	public int Id { get; set; }
	public string PolicyCode { get; set; }
	public string SectionCode { get; set; }
	public string DocumentName { get; set; }
	public string Content { get; set; }
	public Vector Embedding { get; set; }
}
