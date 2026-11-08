using System;
using System.Collections.Generic;

namespace eSufara.Model;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public byte[]? ProfilePicture { get; set; }

    public virtual ICollection<UserProgress> UserProgresses { get; set; } = new List<UserProgress>();
}
