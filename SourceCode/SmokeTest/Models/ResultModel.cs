namespace SmokeTest.Models
{
  public class ResultModel
  {
    public ResultModel(string name)
    {
      Name = name;
      Success = false;
      ErrorMessage = string.Empty;
      ArtifactId = -1;
    }

    public string Name { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int ArtifactId { get; set; }
  }
}
