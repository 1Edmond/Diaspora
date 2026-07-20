using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Dtos;

public sealed record ReactionDto(
    string Emoji,
    int Count,
    bool ReactedByMe
);