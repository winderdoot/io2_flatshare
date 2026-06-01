import { API_URL } from "../config.ts";

type PhotosResponse = {
  listingId: string;
  photos: string[];
};

export const getListingThumbnail = async (
  listingId: string,
  token: string
): Promise<Blob | null> => {
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
      return null;
    }
    
    const photosData: PhotosResponse = await photosResponse.json();

    const firstPhotoId = photosData.photos?.[0];

    if (!firstPhotoId) {
      return null;
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
      return null;
    }

    return await imageResponse.blob();
  } catch {
    return null;
  }
};

export type ListingPhoto = {
  id: string;
  blob: Blob | null;
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

    if (!photosResponse.ok) return [{ id: "default", blob: null }];

    const photosData: PhotosResponse = await photosResponse.json();

    if (!photosData.photos?.length) return [{ id: "default", blob: null }];

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

        if (!imageResponse.ok) return { id: photoId, blob: null };

        const blob = await imageResponse.blob();
        return { id: photoId, blob };
      })
    );

    return images;
  } catch {
    return [{ id: "default", blob: null }];
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