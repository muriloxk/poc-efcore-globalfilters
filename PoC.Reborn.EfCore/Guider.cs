using System;
namespace PoC.Reborn.EfCore
{
    public static class Guider
    {
        private static Guid _testerGuid = Guid.Empty;

        public static Guid GetTesterGuid()
        {
            if (_testerGuid == Guid.Empty)
                _testerGuid = Guid.NewGuid();

            return _testerGuid;
        }
    }
}
