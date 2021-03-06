using System;
using System.Collections.Generic;
using UnityEngine;

using SA.Foundation.Templates;

namespace SA.iOS.UIKit
{
    [Serializable]
    public class ISN_UIPickerControllerResult : SA_Result
    {

        [SerializeField] string m_encodedImage = string.Empty;

        [SerializeField] string m_mediaURL;
        [SerializeField] string m_imageURL;
        [SerializeField] string m_mediaType;

        private Texture2D m_texture = null;


        public ISN_UIPickerControllerResult(SA_Error error):base(error) {}


        /// <summary>
        /// Gets the selected texture.
        /// Value can be <c>null</c> in case user canceled selection, or picked video instead.
        /// </summary>
        /// <value>The texture.</value>
        public Texture2D Image {
            get {
                if(m_texture == null) {
                    if(!string.IsNullOrEmpty(m_encodedImage)) {
                        m_texture = new Texture2D(1, 1);
                        m_texture.LoadImageFromBase64(m_encodedImage);
                    }
                }
                return m_texture;
            }

        }

        /// <summary>
        /// Specifies the media type selected by the user.
        /// The value for this key is an string object containing a type code such 
        /// as <see cref="ISN_UIMediaType.IMAGE"/> or <see cref="ISN_UIMediaType.MOVIE"/>.
        /// </summary>
        public string MediaType {
            get {
                return m_mediaType;
            }
        }


        /// <summary>
        /// Specifies the filesystem URL for the movie.
        /// </summary>
        public string MediaURL {
            get {
                return m_mediaURL;
            }
        }


        /// <summary>
        /// A key containing the URL of the image file.
        /// </summary>
        public string ImageURL {
            get {
                return m_imageURL;
            }
        }
    }
}