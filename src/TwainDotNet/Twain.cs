using System;
using System.Collections.Generic;

namespace TwainDotNet
{
	/// <summary>
	/// Twain.
	/// </summary>
    public class Twain : IDisposable
    {
        DataSourceManager _dataSourceManager;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="messageHook">The message hook.</param>
        public Twain( IWindowsMessageHook messageHook )
        {
            ScanningComplete += delegate
            {
            };
            TransferImage += delegate
            {
            };

            _dataSourceManager = new DataSourceManager( DataSourceManager.DefaultApplicationId, messageHook );
            _dataSourceManager.ScanningComplete += delegate ( object sender, ScanningCompleteEventArgs args )
            {
                ScanningComplete( this, args );
            };
            _dataSourceManager.TransferImage += delegate ( object sender, TransferImageEventArgs args )
            {
                TransferImage( this, args );
            };
        }

        /// <summary>
        /// Notification that the scanning has completed.
        /// </summary>
        public event EventHandler<ScanningCompleteEventArgs> ScanningComplete;

		/// <summary>
		/// Notification that an image has been transferred.
		/// </summary>
        public event EventHandler<TransferImageEventArgs> TransferImage;

        /// <summary>
        /// Starts scanning.
        /// </summary>
		/// <param name="settings">The settings.</param>
        public bool StartScanning( ScanSettings settings )
        {
            return _dataSourceManager.StartScan( settings );
        }

        /// <summary>
        /// Shows a dialog prompting the use to select the source to scan from.
        /// </summary>
        public void SelectSource()
        {
            _dataSourceManager.SelectSource();
        }

        /// <summary>
        /// Selects a source based on the product name string.
        /// </summary>
        /// <param name="sourceName">The source product name.</param>
        public void SelectSource( string sourceName )
        {
            var source = DataSource.GetSource(
                sourceName,
                _dataSourceManager.ApplicationId,
                _dataSourceManager.MessageHook );

            _dataSourceManager.SelectSource( source );
        }

        /// <summary>
        /// Gets the product name for the default source.
        /// </summary>
        public string DefaultSourceName
        {
            get
            {
                using( var source = DataSource.GetDefault( _dataSourceManager.ApplicationId, _dataSourceManager.MessageHook ) )
                {
                    return source.SourceId.ProductName;
                }
            }
        }

        /// <summary>
        /// Gets a list of source product names.
        /// </summary>
        public IList<string> SourceNames
        {
            get
            {
                var result = new List<string>();
                var sources = DataSource.GetAllSources(
                    _dataSourceManager.ApplicationId,
                    _dataSourceManager.MessageHook );

                foreach( var source in sources )
                {
                    result.Add( source.SourceId.ProductName );
                    source.Dispose();
                }

                return result;
            }
        }

		/// <summary>
		/// Get current scan settings.
		/// </summary>
		/// <returns></returns>
        public ScanSettings GetCurrentScanSettings()
        {
            return _dataSourceManager.GetCurrentScanSettings();
        }

		/// <summary>
		/// Finalizer.
		/// </summary>
        ~Twain()
        {
            Dispose( false );
        }

		/// <summary>
		/// Dispose.
		/// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

		/// <summary>
		/// Dispose.
		/// </summary>
		/// <param name="disposing">Whether disposing.</param>
        protected virtual void Dispose( bool disposing )
        {
            if( disposing )
            {
                _dataSourceManager.Dispose();
                _dataSourceManager = null;
            }
        }
    }
}
