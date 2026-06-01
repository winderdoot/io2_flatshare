import rentHouse from "../assets/rent_house.png";
import { API_URL } from "../config.ts";

type PhotosResponse = {
  listingId: string;
  photos: string[];
};

export const getListingThumbnail = async (
  listingId: string,
  token: string
): Promise<string> => {
  try {
    const photosResponse = await fetch(
      `${API_URL}/api/v1/listings/${listingId}/photos`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
          Accept: "application/json",
        },
      }
    );
    
    if (!photosResponse.ok) {
      return rentHouse;
    }
    
    const photosData: PhotosResponse = await photosResponse.json();
    console.log(photosData);

    const firstPhotoId = photosData.photos?.[0];

    if (!firstPhotoId) {
      return rentHouse;
    }

    const imageResponse = await fetch(
      `${API_URL}/api/v1/listings/${listingId}/photos/${firstPhotoId}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
          Accept: "application/json",
        },
      }
    );

    if (!imageResponse.ok) {
      return rentHouse;
    }

    const blob = await imageResponse.blob();

    return URL.createObjectURL(blob);
  } catch {
    return rentHouse;
  }
};

export const getListingPhotos = async (
  listingId: string,
  token: string
): Promise<string[]> => {
  try {
    const photosResponse = await fetch(
      `${API_URL}/api/v1/listings/${listingId}/photos`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
          Accept: "application/json",
        },
      }
    );

    if (!photosResponse.ok) {
      return [rentHouse];
    }

    const photosData: PhotosResponse = await photosResponse.json();

    if (!photosData.photos?.length) {
      return [rentHouse];
    }

    const imageUrls = await Promise.all(
      photosData.photos.map(async (photoId) => {
        const imageResponse = await fetch(
          `${API_URL}/api/v1/listings/${listingId}/photos/${photoId}`,
          {
            headers: {
              Authorization: `Bearer ${token}`,
              Accept: "application/json",
            },
          }
        );

        if (!imageResponse.ok) {
          return rentHouse;
        }

        const blob = await imageResponse.blob();
        return URL.createObjectURL(blob);
      })
    );

    return imageUrls;
  } catch {
    return [rentHouse];
  }
};