namespace SAC9.Parser;

public record Node {
  public int[] Members { get; set; } = new int[2];
  public string Type { get; set; } = null!;
  public List<Node> Childs { get; } = new List<Node>();
}

public record Result {
  public int last { get; set; }
  public string error { get; set; } = string.Empty; 
  public Node ? node { get; set; } = new Node();
}
