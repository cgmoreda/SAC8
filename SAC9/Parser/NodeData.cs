namespace SAC9.Parser;

public record Node {
  public int left { get; set; }
  public int right { get; set; }

  public string Type { get; set; } = string.Empty;

  public List<Node> Children { get; } = new List<Node>();
}

public record Result {
  // -1 if error 
  public int last { get; set; }

  public string error { get; set; } = string.Empty;

  public Node ? node { get; set; } = new Node();
}
