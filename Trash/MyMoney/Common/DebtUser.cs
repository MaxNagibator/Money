using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Common.Enums;
using Extentions;

namespace Common
{
    /// <summary>
    /// Тот кому мы должны или он нам должен
    /// </summary>
    public class DebtUser
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
