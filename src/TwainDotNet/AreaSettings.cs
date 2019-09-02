using System.ComponentModel;

namespace TwainDotNet
{
	/// <summary>
	/// Area settings.
	/// </summary>
    public class AreaSettings : INotifyPropertyChanged
    {
        private float _top;
		/// <summary>
		/// Gets or sets the top.
		/// </summary>
        public float Top
        {
            get { return _top; }
            private set
            {
                _top = value;
                OnPropertyChanged("Top");
            }
        }

        private float _left;
		/// <summary>
		/// Gets or sets the left.
		/// </summary>
		public float Left
        {
            get { return _left; }
            private set
            {
                _left = value;
                OnPropertyChanged("Left");
            }
        }

        private float _bottom;
		/// <summary>
		/// Gets or sets the bottom.
		/// </summary>
		public float Bottom 
        {
            get { return _bottom; }
            private set
            {
                _bottom = value;
                OnPropertyChanged("Bottom");
            }
        }

        private float _right;
		/// <summary>
		/// Gets or sets the right.
		/// </summary>
		public float Right
        {
            get { return _right; }
            private set
            {
                _right = value;
                OnPropertyChanged("Right");
            }
        }

		/// <summary>
		/// Default constructor.
		/// </summary>
		public AreaSettings(float top, float left, float bottom, float right)
        {
            _top = top;
            _left = left;
            _bottom = bottom;
            _right = right;
        }               

        #region INotifyPropertyChanged Members

		/// <summary>
		/// On property changed.
		/// </summary>
		/// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

		/// <summary>
		/// The property changed event handler.
		/// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion
    }
}