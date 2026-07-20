using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Responses;

public sealed record CreateWalletResponse(Guid WalletId);