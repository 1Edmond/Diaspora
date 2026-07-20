using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Enums;

public enum PaymentMode
{
    ContactOnly = 0,       // Mise en relation uniquement
    WalletPayment = 1,     // Via Wallet intégré uniquement
    Both = 2               // Les deux options disponibles
}