import { useQuery } from "@tanstack/react-query";
import { useState, useEffect } from "react";
import { getListingPhotos } from "../images_service/ImagesService";

export const useListingPhotos = (listingId: string) => {
  const token = localStorage.getItem("token") || "";

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ["listing-photos", listingId],
    queryFn: () => getListingPhotos(listingId, token),
    enabled: !!listingId,
    staleTime: 1000 * 60 * 10,
  });

  const [photoUrls, setPhotoUrls] = useState<{ id: string; url: string }[]>([]);

  useEffect(() => {
    if (!data) return;

    const newUrls = data.map((photo) => ({
      id: photo.id,
      url: photo.blob ? URL.createObjectURL(photo.blob) : "",
    }));

    setPhotoUrls(newUrls);

    return () => {
      newUrls.forEach((p) => {
        if (p.url) URL.revokeObjectURL(p.url);
      });
    };
  }, [data]);

  return { 
    photos: photoUrls, 
    isLoading, 
    isError, 
    refetch 
  };
};