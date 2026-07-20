using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Requests;

public sealed record DepositRequest(decimal Amount, string Description);