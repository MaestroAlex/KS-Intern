using System.Threading.Tasks;

namespace ChatMVVM.Models
{
    class Model
    {
        public Model()
        {
        }

        public int IncreaseClicks( int Clicks)
        {
            return Clicks++;
        }
    }
}
