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

export type ListingPhoto = {
  id: string;
  url: string;
};

export const getListingPhotos = async (
  listingId: string,
  token: string
): Promise<ListingPhoto[]> => {
  try {
    const photosResponse = await fetch(
      `${API_URL}/api/v1/listings/${listingId}/photos`,
      {
        cache: "no-store",
        headers: {
          Authorization: `Bearer ${token}`,
          Accept: "application/json",
        },
      }
    );

    if (!photosResponse.ok) return [{ id: "default", url: rentHouse }];

    const photosData: PhotosResponse = await photosResponse.json();

    if (!photosData.photos?.length) return [{ id: "default", url: rentHouse }];

    const images = await Promise.all(
      photosData.photos.map(async (photoId) => {
        const imageResponse = await fetch(
          `${API_URL}/api/v1/listings/${listingId}/photos/${photoId}`,
          {
            cache: "no-store", 
            headers: {
              Authorization: `Bearer ${token}`,
              Accept: "application/json",
            },
          }
        );

        if (!imageResponse.ok) return { id: photoId, url: rentHouse };

        const blob = await imageResponse.blob();
        return { id: photoId, url: URL.createObjectURL(blob) };
      })
    );

    return images;
  } catch {
    return [{ id: "default", url: rentHouse }];
  }
};

export const deleteListingPhoto = async (
  listingId: string,
  photoId: string,
  token: string
): Promise<void> => {
  const response = await fetch(
    `${API_URL}/api/v1/listings/${listingId}/photos/${photoId}`,
    {
      method: "DELETE",
      headers: {
        Authorization: `Bearer ${token}`,
        Accept: "application/json",
      },
    }
  );

  if (!response.ok && response.status !== 204) {
    throw new Error("Error deleting photo");
  }
};

export const uploadListingPhoto = async (
  listingId: string,
  file: File,
  token: string
): Promise<string> => {
  const formData = new FormData();
  formData.append("file", file);

  const response = await fetch(
    `${API_URL}/api/v1/listings/${listingId}/photos`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
        Accept: "application/json",
      },
      body: formData,
    }
  );

  if (!response.ok) {
    throw new Error("Error uploading photo");
  }

  const location = response.headers.get("Location");

  return location ? location.split("/").pop()! : ""; 
};