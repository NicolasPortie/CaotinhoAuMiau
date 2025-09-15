using System;

namespace CaotinhoAuMiau.Models.ViewModels.Comuns
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; } = string.Empty;

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
